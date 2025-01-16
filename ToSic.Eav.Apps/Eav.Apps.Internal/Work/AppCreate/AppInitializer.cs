using System.IO;
using System.Text.RegularExpressions;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repositories;
using static ToSic.Eav.Apps.AppLoadConstants;

namespace ToSic.Eav.Apps.Internal.Work;

/// <summary>
/// The AppInitializer is responsible for ensuring that an App-object has all the properties / metadata it needs. Specifically:
/// - App Configuration (Folder, Version, etc.)
/// - App Resources
/// - App Settings
/// It must be called from an AppManager, which has been created for this app
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppInitializer(
    LazySvc<DataBuilder> builder,
    Generator<IRepositoryLoader> repoLoader,
    GenWorkDb<WorkEntitySave> entitySave,
    GenWorkDb<WorkContentTypesMod> contentTypesMod,
    AppCachePurger cachePurger,
    IAppReaderFactory appReaders)
    : ServiceBase("Eav.AppBld", connect: [contentTypesMod, entitySave, builder, cachePurger, repoLoader, appReaders])
{

    protected readonly AppCachePurger CachePurger = cachePurger;

    /// <summary>
    /// Create app-describing entity for configuration and add Settings and Resources Content Type
    /// </summary>
    /// <param name="appReader">The app State</param>
    /// <param name="newAppName">The app-name (for new apps) which would be the folder name as well. </param>
    /// <param name="codeRefTrail">Origin caller to better track down creation - see issue https://github.com/2sic/2sxc/issues/3203</param>
    public bool InitializeApp(IAppReader appReader, string newAppName, CodeRefTrail codeRefTrail)
    {
        var l = Log.Fn<bool>($"{nameof(newAppName)}: {newAppName}");
        codeRefTrail.WithHere().AddMessage($"App: {appReader.AppId}");
        if (AppInitializedChecker.CheckIfAllPartsExist(appReader, codeRefTrail, out var appConfig, out var appResources,
                out var appSettings, Log))
            return l.ReturnTrue("ok");

        codeRefTrail.AddMessage($"Some parts missing: {nameof(appConfig)}: {appConfig}; {nameof(appResources)}: {appResources}: {nameof(appSettings)}; {appSettings}");

        // Get appName from cache - stop if it's a "Default" app
        var appName = appReader.Specs.NameId;

        // v10.25 from now on the DefaultApp can also have settings and resources
        var folder = PickCorrectFolderName(newAppName, appName);

        var addList = new List<AddContentTypeAndOrEntityTask>();
        if (appConfig == null)
            addList.Add(new(TypeAppConfig,
                values: new()
                {
                    { "DisplayName", newAppName.UseFallbackIfNoValue(appName) },
                    { "Folder", folder },
                    { "AllowTokenTemplates", "True" },
                    { "AllowRazorTemplates", "True" },
                    // always trailing with the version it was created with
                    // Note that v13 and 14 both report v13, only 15+ uses the real version
                    { "Version", $"00.00.{EavSystemInfo.Version.Major:00}" },
                    { "OriginalId", "" },
                    // 2023-11-08 2dm - https://github.com/2sic/2sxc/issues/3203
                    { "DebugLog", codeRefTrail.ToString() },
                },
                false));


        // Add new (empty) ContentType for Settings
        if (appSettings == null)
            addList.Add(new(TypeAppSettings));

        // add new (empty) ContentType for Resources
        if (appResources == null)
            addList.Add(new(TypeAppResources));

        // If the Types are missing, create these first
        if (CreateAllMissingContentTypes(appReader, addList))
        {
            // since the types were re-created, we must flush it from the cache
            // this is because other APIs may access the AppStates (though they shouldn't)
            CachePurger.Purge(appReader);
            // get the latest app-state, but not-initialized so we can make changes
            appReader = repoLoader.New().AppStateBuilderRaw(appReader.AppId, codeRefTrail.WithHere()).Reader;
        }

        addList.ForEach(task => MetadataEnsureTypeAndSingleEntity(appReader, task));

        // Reset App-State to ensure it's reloaded with the added configuration
        CachePurger.Purge(appReader);

        return l.ReturnFalse("ok");
    }

    private static string PickCorrectFolderName(string newAppName, string eavAppName) =>
        eavAppName switch
        {
            Constants.DefaultAppGuid => Constants.ContentAppFolder,
            Constants.PrimaryAppGuid or Constants.PrimaryAppName => Constants.PrimaryAppName,
            _ => string.IsNullOrEmpty(newAppName) ? eavAppName : RemoveIllegalCharsFromPath(newAppName)
        };


    private bool CreateAllMissingContentTypes(IAppReader appState, List<AddContentTypeAndOrEntityTask> newItems)
    {
        var l = Log.Fn<bool>($"Check for {newItems.Count}");
        var typesMod = contentTypesMod.New(appState);
        var addedTypes = false;
        foreach (var item in newItems)
            if (item.InAppType && FindContentType(appState, item.SetName, item.InAppType) == null)
            {
                l.A("couldn't find type, will create");
                // create App-Man if not created yet
                typesMod.Create(item.SetName, Scopes.App);
                addedTypes = true;
            }
            else
                l.A($"Type '{item.SetName}' found");

        return l.Return(addedTypes);
    }
        
    private void MetadataEnsureTypeAndSingleEntity(IAppReader appStateRaw, AddContentTypeAndOrEntityTask cTypeAndOrEntity)
    {
        var l = Log.Fn($"{cTypeAndOrEntity.SetName} for app {appStateRaw.AppId} - inApp: {cTypeAndOrEntity.InAppType}");
        var ct = FindContentType(appStateRaw, cTypeAndOrEntity.SetName, cTypeAndOrEntity.InAppType);

        // if it's still null, we have a problem...
        if (ct == null)
        {
            l.A("type is still null, error");
            throw l.Done(new Exception("something went wrong - can't find type in app, but it's not a global type, so I must cancel"));
        }

        var values = cTypeAndOrEntity.Values ?? [];
        var attrs = builder.Value.Attribute.Create(values);
        var mdTarget = new Target((int)TargetTypes.App, "App", keyNumber: appStateRaw.AppId);
        var newEnt = builder.Value.Entity
            .Create(appId: appStateRaw.AppId, guid: Guid.NewGuid(), contentType: ct, attributes: attrs, metadataFor: mdTarget);

        var entSaver = entitySave.New(appStateRaw);
        entSaver.Save(newEnt, entSaver.SaveOptions());
        l.Done();
    }

    private IContentType FindContentType(IAppReadContentTypes appStateRaw, string setName, bool inAppType)
    {
        // if it's an in-app type, it should check the app, otherwise it should check the global type
        // we're NOT asking the app for all types (which would be the normal way)
        // because there are rare cases where historic data accidentally
        // created the 2SexyContent-App type as a local type in an app (2sxc 9.20-9.22)
        // Basically after this update has run for a while - probably till end of 2018-04
        // this is probably not so important any more, but I would leave it forever for now
        // discuss w/2dm if you think you want to change this
        var ct = inAppType
            ? appStateRaw.GetContentType(setName)
            : appReaders.GetSystemPreset().GetContentType(setName);
        return ct;
    }

    private static string RemoveIllegalCharsFromPath(string path)
    {
        var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        var r = new Regex($"[{Regex.Escape(regexSearch)}]");
        return r.Replace(path, "");
    }


    private class AddContentTypeAndOrEntityTask(
        string setName,
        Dictionary<string, object> values = null,
        bool inAppType = true)
    {
        public readonly string SetName = setName;
        public readonly Dictionary<string, object> Values = values;
        public readonly bool InAppType = inAppType;
    }
}