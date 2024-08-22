using System.Diagnostics;
using System.Text.Json;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.Persistence.Efc;

/// <summary>
/// Will load all DB data into the memory data model.
/// It uses Entity Framework Core 1.1 which we use for .net 451 (DNN)
/// It also works with the last Entity Framework 3, which we use for Oqtane etc.
/// </summary>
partial class Efc11Loader
{
    #region AppPackage

    /// <summary>
    /// Load the full AppState from the backend - in an un-initialized state (without folder / name etc.).
    /// This is mostly for internal operations where initialization would cause trouble or unexpected side-effects.
    /// </summary>
    /// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
    /// <param name="codeRefTrail"></param>
    /// <returns>An object with everything which an app has, usually for caching</returns>
    private IAppStateBuilder LoadAppStateFromDb(int appId, CodeRefTrail codeRefTrail)
    {
        logStore.Add(EavLogs.LogStoreAppStateLoader, Log);

        var l = Log.Fn<IAppStateBuilder>($"AppId: {appId}");
        var appIdentity = appsCatalog.AppIdentity(appId);
        var appGuidName = appsCatalog.AppNameId(appIdentity.ZoneId, appIdentity.AppId);
        codeRefTrail.WithHere().AddMessage($"App: {appId}, {nameof(appGuidName)}: '{appGuidName}'");

        // This will contain the parent reference - in most cases it's the -42 App
        ParentAppState parent;

        var ancestorAppId = GetAncestorAppIdOrZero(appId);

        if (ancestorAppId != 0)
        {
            // Check if feature is enabled #SharedAppFeatureEnabled
            if (!featuresService.Value.IsEnabled(BuiltInFeatures.SharedApps))
                throw new FeaturesDisabledException(BuiltInFeatures.SharedApps.NameId, 
                    $"This is required to load shared app states. " +
                    $"The App {appIdentity.Show()} has an ancestor {ancestorAppId}. " +
                    $"This implies that it has an ancestor. 0 was expected, otherwise you need the feature.");

            codeRefTrail.AddMessage($"Ancestor: {ancestorAppId}");
            var testParentApp = appStates.GetCacheState(ancestorAppId);
            parent = new(testParentApp, true, true);
        }
        else
        {
            // New v13 - use global app by default to share content-types
            var globalApp = appStates.Get(Constants.PresetIdentity);
            parent = new(globalApp ?? throw new("Can't find global app - which is required to build any other apps. "),
                true, 
                false);
        }

        var builder = appStateBuilder.New().InitForNewApp(parent, appIdentity, appGuidName, Log);
        Update(builder.AppState, AppStateLoadSequence.Start, codeRefTrail);

        return l.ReturnAsOk(builder);
    }

    private int GetAncestorAppIdOrZero(int appId)
    {
        var l = Log.Fn<int>($"{nameof(appId)}:{appId}");
        // Prefetch this App (new in v13 for ancestor apps)
        var appInDb = context.ToSicEavApps.FirstOrDefault(a => a.AppId == appId);
        var appSysSettings = appInDb?.SysSettings;
        if (string.IsNullOrWhiteSpace(appSysSettings))
            return l.Return(0, "none found");

        var sysSettings = JsonSerializer.Deserialize<AppSysSettings>(appInDb.SysSettings, JsonOptions.SafeJsonForHtmlAttributes);
        if (!sysSettings.Inherit || sysSettings.AncestorAppId == 0) 
            return l.Return(0, "data found but inherit not active");

        if (sysSettings.AncestorAppId == appId)
        {
            Log.A($"Error: Got an {nameof(sysSettings.AncestorAppId)} of {appId}. " +
                  "It's the same as the app itself - this should never happen. Stop.");
            return l.Return(0, "error");
        }

        return l.Return(sysSettings.AncestorAppId, $"found {sysSettings.AncestorAppId}");
    }

    public IAppStateBuilder AppStateBuilderRaw(int appId, CodeRefTrail codeRefTrail)
    {
        var l = Log.Fn<IAppStateBuilder>($"{appId}", timer: true);
        codeRefTrail.WithHere();
        var builder = LoadAppStateFromDb(appId, codeRefTrail);
        return l.ReturnAsOk(builder);
    }

    /// <inheritdoc />
    public IAppStateCache AppStateInitialized(int appId, CodeRefTrail codeRefTrail)
    {
        // Note: Ignore ensureInitialized on the content app
        // The reason is that this app - even when empty - is needed in the cache before data is imported
        // So if we initialize it, then things will result in duplicate settings/resources/configuration
        // Note that to ensure the Content app works, we must perform the same check again in the 
        // API Endpoint which will edit this data

        var l = Log.Fn<IAppStateCache>($"{appId}", timer: true);

        var builder = AppStateBuilderRaw(appId, codeRefTrail.WithHere().AddMessage("First Build")); // LoadAppStateFromDb(appId);

        if (builder.Reader.NameId == Constants.DefaultAppGuid)
            return l.Return(builder.AppState, "default app, don't auto-init");

        var result = initializedChecker.EnsureAppConfiguredAndInformIfRefreshNeeded(builder.Reader, null, codeRefTrail.WithHere(), Log)
            //? LoadAppStateFromDb(appId).AppState
            ? AppStateBuilderRaw(appId, codeRefTrail.WithHere()).AppState
            : builder.AppState;

        return l.Return(result, "with init check");
    }

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
                _sqlTotalTime = _sqlTotalTime.Add(InitMetadataLists(builder));
                var nameAndFolder = PreLoadAppPath(state.AppId);
                builder.SetNameAndFolder(nameAndFolder.Name, nameAndFolder.Path);
            }
            else
                l.A("skipping metadata load");

            // 2024-01-12 2dm - this doesn't seem to do much, disable and remove ca. 2024-Q2
            //if (startAt <= AppStateLoadSequence.ContentTypeLoad)
            //    startAt = AppStateLoadSequence.ContentTypeLoad;

            // prepare content-types
            if (startAt <= AppStateLoadSequence.ContentTypeLoad)
            {
                var typeTimer = Stopwatch.StartNew();
                var dbTypes = ContentTypes(state.AppId, state);
                dbTypes = LoadExtensionsTypesAndMerge(builder.Reader, dbTypes);
                builder.InitContentTypes(dbTypes);
                typeTimer.Stop();
                l.A($"timers types:{typeTimer.Elapsed}");
            }
            else
                l.A("skipping content-type load");

            // load data
            if (startAt <= AppStateLoadSequence.ItemLoad)
                LoadEntities(builder, codeRefTrail, entityIds);
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
            var dbEntity = GetRawEntities(Array.Empty<int>(), appId, false, AppLoadConstants.TypeAppConfig);
            if (!dbEntity.Any()) return l.Return(nullTuple, "not in db");

            // Get the first one as it should be the one containing the App-Configuration
            // WARNING: This looks a bit fishy, I think it shouldn't just assume the first one is the right one
            var json = dbEntity.FirstOrDefault()?.Json;
            if (string.IsNullOrEmpty(json)) return l.Return(nullTuple, "no json");

            l.A("app Entity found - this json: " + json);
            var serializer = dataDeserializer.New(); // ServiceProvider.Build<IDataDeserializer>();
            serializer.Initialize(appId, new List<IContentType>(), null);
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

}