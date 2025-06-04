using System.Text.Json;
using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Context;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Entities.Sys;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Metadata.Sys;
using ToSic.Eav.Persistence.Sys.AppState;
using ToSic.Eav.Persistence.Sys.Loaders;
using ToSic.Eav.Repositories;
using ToSic.Eav.Serialization;
using ToSic.Eav.Serialization.Sys.Json;
using ToSic.Eav.Sys;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Persistence.Efc;

/// <summary>
/// Loader of an App, it's ContentTypes, Entities etc. from SQL using Entity Framework Core.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class EfcAppLoader(
    EavDbContext context,
    LazySvc<IZoneCultureResolver> environmentLazy,
    IAppInitializedChecker initializedChecker,
    IAppsCatalog appsCatalog,
    IAppStateCacheService appStates,
    ILogStore logStore,
    ISysFeaturesService featuresSvc,
    DataBuilder dataBuilder,
    Generator<IDataDeserializer> dataDeserializer,
    Generator<IAppContentTypesLoader> appFileContentTypesLoader,
    Generator<IAppStateBuilder> appStateBuilder)
    : ServiceBase("Db.Efc11",
        connect:
        [
            context, environmentLazy, initializedChecker, appsCatalog, appStates, logStore, featuresSvc, dataBuilder,
            dataDeserializer, appFileContentTypesLoader, appStateBuilder
        ]), IAppsAndZonesLoaderWithRaw
{
    #region Setup, SQL Timer, Primary Language
    
    public EfcAppLoader UseExistingDb(EavDbContext dbContext)
    {
        Context = dbContext;
        return this;
    }

    private TimeSpan _sqlTotalTime = new(0);

    internal void AddSqlTime(TimeSpan sqlTime) => _sqlTotalTime = _sqlTotalTime.Add(sqlTime);

    internal EavDbContext Context { get; private set; } = context;

    /// <summary>
    /// The current primary language - mainly for ordering values with primary language first
    /// </summary>
    public string PrimaryLanguage
    {
        get
        {
            if (field != null)
                return field;
            var l = Log.Fn<string>();
            field = environmentLazy.Value.DefaultCultureCode.ToLowerInvariant();
            return l.Return(field, $"Primary language from Env (for value sorting): {field}");
        }
        set;
    }

    #endregion

    #region LogSettings


    public LogSettings LogSettings => field
        ??= new AppLoaderLogSettings(featuresSvc).GetLogSettings();
    
    #endregion

    #region IRepositoryLoader Zones and ContentTypes

    IDictionary<int, Zone> IAppsAndZonesLoader.Zones()
        => new ZoneLoader(this).LoadZones(logStore);

    /// <inheritdoc />
    /// <summary>
    /// Get all ContentTypes for specified AppId. 
    /// It uses temporary caching, so if called multiple times it loads from a private field.
    /// </summary>
    IList<IContentType> IContentTypeLoader.ContentTypes(int appId, IHasMetadataSourceAndExpiring source)
        => new ContentTypeLoader(this, appFileContentTypesLoader, dataDeserializer, dataBuilder, appStates, featuresSvc)
            .LoadContentTypesFromDb(appId, source);

    #endregion


    #region AppPackage Loader

    /// <inheritdoc />
    IAppStateBuilder IAppsAndZonesLoaderWithRaw.AppStateRawBuilder(int appId, CodeRefTrail codeRefTrail)
    {
        var l = Log.Fn<IAppStateBuilder>($"{appId}", timer: true);
        codeRefTrail.WithHere();
        var builder = LoadAppStateRawFromDb(appId, codeRefTrail);
        return l.ReturnAsOk(builder);
    }

    IAppReader IAppsAndZonesLoaderWithRaw.AppReaderRaw(int appId, CodeRefTrail codeRefTrail)
        => LoadAppStateRawFromDb(appId, codeRefTrail).Reader;

    /// <inheritdoc />
    IAppStateCache IAppsAndZonesLoader.AppState(int appId, CodeRefTrail codeRefTrail)
    {
        // Note: Ignore ensureInitialized on the content app
        // The reason is that this app - even when empty - is needed in the cache before data is imported
        // So if we initialize it, then things will result in duplicate settings/resources/configuration
        // Note that to ensure the Content app works, we must perform the same check again in the 
        // API Endpoint which will edit this data

        var l = Log.Fn<IAppStateCache>($"{appId}", timer: true);

        var builder = LoadAppStateRawFromDb(appId, codeRefTrail.WithHere().AddMessage("First Build"));

        if (builder.Reader.Specs.IsContentApp())
            return l.Return(builder.AppState, "default app, don't auto-init");

        var needsReload = initializedChecker
            .EnsureAppConfiguredAndInformIfRefreshNeeded(builder.Reader, null, codeRefTrail.WithHere(), Log);

        if (!needsReload)
            return l.Return(builder.AppState, "with init check, no reload needed");

        var reloaded = LoadAppStateRawFromDb(appId, codeRefTrail.WithHere()).AppState;
        return l.Return(reloaded, "with init check; reloaded");
    }


    /// <summary>
    /// Load the full AppState from the backend - in an un-initialized state (without folder / name etc.).
    /// This is mostly for internal operations where initialization would cause trouble or unexpected side effects.
    /// </summary>
    /// <param name="appId">AppId (can be different from the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
    /// <param name="codeRefTrail"></param>
    /// <returns>An object with everything which an app has, usually for caching</returns>
    private IAppStateBuilder LoadAppStateRawFromDb(int appId, CodeRefTrail codeRefTrail)
    {
        var logStoreEntry = logStore.Add(EavLogs.LogStoreAppStateLoader, Log);


        var l = Log.Fn<IAppStateBuilder>($"AppId: {appId}");
        var appIdentity = appsCatalog.AppIdentity(appId);
        var appGuidName = appsCatalog.AppNameId(appIdentity);
        logStoreEntry?.AddSpec("App", $"{appIdentity.Show()}");
        logStoreEntry?.AddSpec("App NameId", appGuidName);
        codeRefTrail.WithHere().AddMessage($"App: {appId}, {nameof(appGuidName)}: '{appGuidName}'");

        // This will contain the parent reference - in most cases it's the -42 App
        ParentAppState? parent;

        var ancestorAppId = GetAncestorAppIdOrZero(appId);

        if (ancestorAppId != 0)
        {
            // Check if feature is enabled #SharedAppFeatureEnabled
            if (!featuresSvc.IsEnabled(BuiltInFeatures.SharedApps))
                throw new FeaturesDisabledException(BuiltInFeatures.SharedApps.NameId,
                    $"This is required to load shared app states. " +
                    $"The App {appIdentity.Show()} has an ancestor {ancestorAppId}. " +
                    $"This implies that it has an ancestor. 0 was expected, otherwise you need the feature.");

            codeRefTrail.AddMessage($"Ancestor: {ancestorAppId}");
            var testParentApp = appStates.Get(ancestorAppId);
            parent = new(testParentApp, true, true);
        }
        else
        {
            // New v13 - use global app by default to share content-types
            var globalApp = appStates.Get(KnownAppsConstants.PresetIdentity);
            parent = new(globalApp ?? throw new("Can't find global app - which is required to build any other apps. "),
                true,
                false);
        }

        var builder = appStateBuilder.New().InitForNewApp(parent, appIdentity, appGuidName, Log);
        Update(builder.AppState, AppStateLoadSequence.Start, codeRefTrail);

        return l.ReturnAsOk(builder);
    }

    /// <inheritdoc />
    public IAppStateCache Update(IAppStateCache appStateOriginal, AppStateLoadSequence startAt, CodeRefTrail codeRefTrail, int[] entityIds = null)
    {
        var lMain = Log.Fn<IAppStateCache>(message: "What happens inside this is logged in the app-state loading log");
        codeRefTrail.WithHere().AddMessage($"App: {appStateOriginal.AppId}");
        var builder = appStateBuilder.New().Init(appStateOriginal);
        builder.Load($"startAt: {startAt}, ids only:{entityIds != null}", state =>
        {
            var l = Log.Fn();
            codeRefTrail.WithHere();
            // prepare metadata lists & relationships etc.
            if (startAt <= AppStateLoadSequence.MetadataInit)
            {
                AddSqlTime(InitMetadataLists(builder));
                var (name, path) = PreLoadAppPath(state.AppId);
                builder.SetNameAndFolder(name, path);
            }
            else
                l.A("skipping metadata load");

            // prepare content-types
            if (startAt <= AppStateLoadSequence.ContentTypeLoad)
            {
                var typeTimer = Stopwatch.StartNew();
                var loader = new ContentTypeLoader(this, appFileContentTypesLoader, dataDeserializer, dataBuilder, appStates, featuresSvc);
                var dbTypesPreMerge = loader.LoadContentTypesFromDb(state.AppId, state);
                var dbTypes = loader.LoadExtensionsTypesAndMerge(builder.Reader, dbTypesPreMerge);
                builder.InitContentTypes(dbTypes);
                typeTimer.Stop();
                l.A($"timers types:{typeTimer.Elapsed}");
            }
            else
                l.A("skipping content-type load");

            // load data
            if (startAt <= AppStateLoadSequence.ItemLoad)
                LoadEntities(builder, codeRefTrail, entityIds ?? []);
            else
            {
                codeRefTrail.AddMessage("skipping items load");
                l.A("skipping items load");
            }

            l.Done($"timers sql:sqlAll:{_sqlTotalTime}");
        });

        return lMain.ReturnAsOk(builder.AppState);
    }

    /// <summary>
    /// Find the ID of the ancestor App (if any), otherwise return 0.
    /// </summary>
    private int GetAncestorAppIdOrZero(int appId)
    {
        var l = Log.Fn<int>($"{nameof(appId)}:{appId}");
        // Prefetch this App (new in v13 for ancestor apps)
        var appInDb = Context.TsDynDataApps.FirstOrDefault(a => a.AppId == appId);
        var appSysSettings = appInDb?.SysSettings;
        if (string.IsNullOrWhiteSpace(appSysSettings))
            return l.Return(0, "none found");

        var sysSettings = JsonSerializer.Deserialize<AppSysSettingsJsonInDb>(appInDb.SysSettings, JsonOptions.SafeJsonForHtmlAttributes);
        if (!sysSettings.Inherit || sysSettings.AncestorAppId == 0)
            return l.Return(0, "data found but inherit not active");

        if (sysSettings.AncestorAppId != appId)
            return l.Return(sysSettings.AncestorAppId, $"found {sysSettings.AncestorAppId}");

        l.A($"Error: Got an {nameof(sysSettings.AncestorAppId)} of {appId}. It's the same as the app itself - this should never happen. Stop.");
        return l.Return(0, "error");

    }


    /// <summary>
    /// Must load the app-path from the settings early on, so that other loaders have it
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    private (string Name, string Path) PreLoadAppPath(int appId)
    {
        var l = Log.Fn<(string Name, string Path)>($"{nameof(appId)}: {appId}");
        var nullTuple = (null as string, null as string);
        try
        {
            // Get all Entities in the 2SexyContent-App scope
            var entityLoader = new EntityLoader(this, dataDeserializer, dataBuilder, featuresSvc);
            var dbEntity = entityLoader.LoadEntitiesFromDb(appId, [], AppLoadConstants.TypeAppConfig);
            if (dbEntity.Count == 0)
                return l.Return(nullTuple, "not in db");

            // Get the first one as it should be the one containing the App-Configuration
            // WARNING: This looks a bit fishy, I think it shouldn't just assume the first one is the right one
            var json = dbEntity.FirstOrDefault()?.Json;
            if (string.IsNullOrEmpty(json))
                return l.Return(nullTuple, "no json");

            l.A("app Entity found - this json: " + json);
            var serializer = dataDeserializer.New();
            serializer.Initialize(appId, [], null);
            if (serializer.Deserialize(json, true, true) is not Entity appEntity)
                return l.Return(nullTuple, "can't deserialize");
            var path = appEntity.Get<string>(AppLoadConstants.FieldFolder);
            var name = appEntity.Get<string>(AppLoadConstants.FieldName);

            return string.IsNullOrWhiteSpace(path)
                ? l.Return((name, path), "no folder")
                : l.Return((name, path), path);
        }
        catch (Exception ex)
        {
            // Ignore, but log
            l.Ex(ex);
        }

        return l.Return(nullTuple, "error");
    }


    #endregion

    #region App Entities

    internal void LoadEntities(IAppStateBuilder builder, CodeRefTrail codeRefTrail, int[] entityIds)
    {
        var entityLoader = new EntityLoader(this, dataDeserializer, dataBuilder, featuresSvc);
        var entitySqlTime = entityLoader.LoadEntities(builder, codeRefTrail, entityIds);
        AddSqlTime(entitySqlTime);
    }

    #endregion

    #region App Metadata

    internal TimeSpan InitMetadataLists(IAppStateBuilder builder)
    {
        var l = Log.Fn<TimeSpan>($"{builder.AppState.Show()}", timer: true);
        builder.InitMetadata();
        return l.Return(l.Timer.Elapsed);
    }

    #endregion


}