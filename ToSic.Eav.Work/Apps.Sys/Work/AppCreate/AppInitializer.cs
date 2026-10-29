using System.Text.RegularExpressions;
using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Targets;
using ToSic.Eav.Sys;
using ToSic.Sys.Utils;
using static ToSic.Eav.Apps.Sys.AppLoadConstants;

namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// The AppInitializer is responsible for ensuring that an App-object has all the properties / metadata it needs. Specifically:
/// - App Configuration (Folder, Version, etc.)
/// - App Resources
/// - App Settings
/// It must be called from an AppManager, which has been created for this app
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppInitializer(
    LazySvc<DataAssembler> builder,
    Generator<IAppsAndZonesLoaderWithRaw> repoLoader,
    AppWorkChain<WorkEntitySave> entitySave,
    AppWorkChain<WorkContentTypesMod> contentTypesMod,
    AppCachePurger cachePurger,
    IAppReaderFactory appReaders)
    : ServiceWithSetup<IAppWorkContext>("Eav.AppBld", connect: [contentTypesMod, entitySave, builder, cachePurger, repoLoader, appReaders])
{

    protected readonly AppCachePurger CachePurger = cachePurger;

    /// <summary>
    /// Create app-describing entity for configuration and add Settings and Resources Content Type
    /// </summary>
    /// <param name="newAppName">The app-name (for new apps) which would be the folder name as well. </param>
    /// <param name="codeRefTrail">Origin caller to better track down creation - see issue https://github.com/2sic/2sxc/issues/3203</param>
    public bool InitializeApp(string? newAppName, CodeRefTrail codeRefTrail)
    {
        var l = Log.Fn<bool>($"{nameof(newAppName)}: {newAppName}");
        var appReader = MyOptions.AppReader;
        codeRefTrail.WithHere().AddMessage($"App: {appReader.AppId}");
        
        // If all parts already exist, exit early.
        if (AppInitializedChecker.CheckIfAllPartsExist(appReader, codeRefTrail, out var appConfig, out var appResources,
                out var appSettings, Log))
            return l.ReturnTrue("ok");

        codeRefTrail.AddMessage($"Some parts missing: {nameof(appConfig)}: {appConfig}; {nameof(appResources)}: {appResources}: {nameof(appSettings)}; {appSettings}");

        // Get appName from cache/specs
        var appName = appReader.Specs.NameId;

        // Start with the list of things to add
        // If the appConfig is null, we need to add it, otherwise we don't.
        List<AddContentTypeAndOrEntityTask> addList = appConfig != null
            ? []
            :
            [
                new(TypeAppConfig, Values: new()
                {
                    { "DisplayName", newAppName.UseFallbackIfNoValue(appName) },
                    { "Folder", PickCorrectFolderName(newAppName, appName) },
                    { AppConfigurationFields.FieldAllowToken, "True" },
                    { AppConfigurationFields.FieldAllowRazor, "True" },
                    // always trailing with the version it was created with
                    // Note that v13 and 14 both report v13, only 15+ uses the real version
                    { "Version", $"00.00.{EavSystemInfo.Version.Major:00}" },
                    { "OriginalId", "" },
                    // 2023-11-08 2dm - https://github.com/2sic/2sxc/issues/3203
                    { "DebugLog", codeRefTrail.ToString() },
                }, false)
            ];


        // Add new (empty) ContentType for Settings
        if (appSettings == null)
            addList.Add(new(TypeAppSettings, []));

        // add new (empty) ContentType for Resources
        if (appResources == null)
            addList.Add(new(TypeAppResources, []));

        // If any of the type definitions are missing, create these first
        if (CreateAllMissingContentTypes(addList))
        {
            // since the types were re-created, we must flush it from the cache
            // this is because other APIs may access the AppStates (though they shouldn't)
            CachePurger.Purge(appReader);
            // get the latest app-state, but not fully initialized so we can make changes
            appReader = repoLoader.New().AppReaderRaw(appReader.AppId, codeRefTrail.WithHere());
        }

        // Check if we have anything to add
        // By reasoning it must always be true, otherwise we would have exited early
        // so this condition is actually a bit irrelevant, but we're just making sure
        if (addList.Any())
        {
            // Create a new context and new DB connection, using the latest AppReader
            var newContext = MyOptions.FreshContext(appReader);
            var entSaver = entitySave.New(newContext);
            var saveOptions = entSaver.SaveOptions();
            
            // Create list to add with save options
            var entitySavePairs = addList
                .Select(addTask => AppMetadataEnsureTypeAndConstructEntityToAdd(appReader, addTask))
                .Select(e => new EntityPair<SaveOptions>(e, saveOptions))
                .ToListOpt();
            entSaver.Save(entitySavePairs);
        }

        // Reset App-State to ensure it's reloaded with the added configuration
        CachePurger.Purge(appReader);

        return l.ReturnFalse("ok");
    }

    private static string PickCorrectFolderName(string? newAppName, string eavAppName) =>
        eavAppName switch
        {
            KnownAppsConstants.DefaultAppGuid => KnownAppsConstants.ContentAppFolder,
            KnownAppsConstants.PrimaryAppGuid or KnownAppsConstants.PrimaryAppName => KnownAppsConstants.PrimaryAppName,
            _ => string.IsNullOrEmpty(newAppName)
                ? eavAppName
                : RemoveIllegalCharsFromPath(newAppName!)
        };


    private bool CreateAllMissingContentTypes(List<AddContentTypeAndOrEntityTask> newItems)
    {
        var l = Log.Fn<bool>($"Check for {newItems.Count}");
        var typesMod = contentTypesMod.New(MyOptions);
        var addedTypes = false;
        foreach (var item in newItems)
            if (item.InAppType && FindContentType(MyOptions.AppReader, item.SetName, item.InAppType) == null)
            {
                l.A("couldn't find type, will create");
                // create App-Man if not created yet
                typesMod.Create(item.SetName, ScopeConstants.App);
                addedTypes = true;
            }
            else
                l.A($"Type '{item.SetName}' found");

        return l.Return(addedTypes);
    }
        
    private Entity AppMetadataEnsureTypeAndConstructEntityToAdd(IAppReader appReader, AddContentTypeAndOrEntityTask cTypeAndOrEntity)
    {
        var l = Log.Fn<Entity>($"{cTypeAndOrEntity.SetName} for app {appReader.AppId} - inApp: {cTypeAndOrEntity.InAppType}");
        var ct = FindContentType(appReader, cTypeAndOrEntity.SetName, cTypeAndOrEntity.InAppType);

        // if it's still null, we have a problem...
        if (ct == null)
        {
            l.A("type is still null, error");
            throw l.Done(new Exception("something went wrong - can't find type in app, but it's not a global type, so I must cancel"));
        }

        var attrs = builder.Value.AttributeList.Finalize(cTypeAndOrEntity.Values!);
        var mdTarget = new Target((int)TargetTypes.App, "App", keyNumber: appReader.AppId);
        var newEnt = builder.Value.Entity
            .Create(appId: appReader.AppId, guid: Guid.NewGuid(), contentType: ct, attributes: attrs, metadataFor: mdTarget);

        return l.Return(newEnt);
    }

    /// <summary>
    /// Get the content type.
    /// WARNING: this is called once with the old reader, and once with new, so it must absolutely not use the appReader from MyOptions, but the one passed in as parameter
    /// </summary>
    /// <param name="currentReader">Current app reader</param>
    /// <param name="typeName"></param>
    /// <param name="inAppType"></param>
    /// <returns></returns>
    private IContentType? FindContentType(IAppReader currentReader, string typeName, bool inAppType)
    {
        // if it's an in-app type, it should check the app, otherwise it should check the global type
        // we're NOT asking the app for all types (which would be the normal way)
        // because there are rare cases where historic data accidentally
        // created the 2SexyContent-App type as a local type in an app (2sxc 9.20-9.22)
        // Basically after this update has run for a while - probably till end of 2018-04
        // this is probably not so important anymore, but I would leave it forever for now
        // discuss w/2dm if you think you want to change this

        // Avoid recursive loading of the preset app (-42) which causes repeated DbContext creations and connection exhaustion
        if (currentReader.AppId == KnownAppsConstants.PresetAppId
            || currentReader.AppId == KnownAppsConstants.GlobalPresetAppId)
            return currentReader.TryGetContentType(typeName);

        var ct = inAppType
            ? currentReader.TryGetContentType(typeName)
            : appReaders.GetSystemPreset().TryGetContentType(typeName);
        return ct;
    }

    private static string RemoveIllegalCharsFromPath(string path)
    {
        var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        var r = new Regex($"[{Regex.Escape(regexSearch)}]");
        return r.Replace(path, "");
    }


    private record AddContentTypeAndOrEntityTask(string SetName, Dictionary<string, object> Values, bool InAppType = true);
}
