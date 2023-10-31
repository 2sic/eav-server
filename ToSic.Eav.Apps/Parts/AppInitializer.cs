using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ToSic.Eav.Apps.AppSys;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repositories;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// The AppInitializer is responsible for ensuring that an App-object has all the properties / metadata it needs. Specifically:
    /// - App Configuration (Folder, Version, etc.)
    /// - App Resources
    /// - App Settings
    /// It must be called from an AppManager, which has been created for this app
    /// </summary>
    public class AppInitializer : ServiceBase
    {

        #region Constructor / DI

        public AppInitializer(
            LazySvc<DataBuilder> builder,
            Generator<IRepositoryLoader> repositoryLoaderGenerator,
            AppWork appWork,
            Generator<AppManager> appManagerGenerator,
            SystemManager systemManager,
            IAppStates appStates) : base("Eav.AppBld")
        {
            ConnectServices(
                _appWork = appWork,
                _builder = builder,
                SystemManager = systemManager,
                _repositoryLoaderGenerator = repositoryLoaderGenerator,
                _appManagerGenerator = appManagerGenerator,
                _appStates = appStates
            );
        }

        private readonly AppWork _appWork;
        private readonly LazySvc<DataBuilder> _builder;
        private readonly Generator<AppManager> _appManagerGenerator;
        private readonly Generator<IRepositoryLoader> _repositoryLoaderGenerator;
        private readonly IAppStates _appStates;
        protected readonly SystemManager SystemManager;


        //public AppInitializer Init(AppState appState)
        //{
        //    AppState = appState;
        //    return this;
        //}

        //private AppState AppState { get; set; }

        ///// <summary>
        ///// The App Manager must be re-created during initialization
        ///// So we don't inject into into this class, but instead create it on demand
        ///// </summary>
        //private AppManager AppManager =>
        //    _appManager ?? (_appManager = _appManagerGenerator.New().InitWithState(AppState, true));
        //private AppManager _appManager;


        #endregion

        /// <summary>
        /// Create app-describing entity for configuration and add Settings and Resources Content Type
        /// </summary>
        /// <param name="appState">The app State</param>
        /// <param name="newAppName">The app-name (for new apps) which would be the folder name as well. </param>
        public bool InitializeApp(AppState appState, string newAppName = null) => Log.Func($"{nameof(newAppName)}: {newAppName}", () =>
        {
            if (AppInitializedChecker.CheckIfAllPartsExist(appState, out var appConfig, out var appResources,
                    out var appSettings, Log))
                return (true, "ok");

            // Get appName from cache - stop if it's a "Default" app
            var eavAppName = appState.NameId;

            // v10.25 from now on the DefaultApp can also have settings and resources
            var folder = PickCorrectFolderName(newAppName, eavAppName);

            var addList = new List<AddContentTypeAndOrEntityTask>();
            if (appConfig == null)
                addList.Add(new AddContentTypeAndOrEntityTask(AppLoadConstants.TypeAppConfig,
                    values: new Dictionary<string, object>
                    {
                        { "DisplayName", string.IsNullOrEmpty(newAppName) ? eavAppName : newAppName },
                        { "Folder", folder },
                        { "AllowTokenTemplates", "True" },
                        { "AllowRazorTemplates", "True" },
                        // always trailing with the version it was created with
                        // Note that v13 and 14 both report v13, only 15+ uses the real version
                        { "Version", $"00.00.{EavSystemInfo.Version.Major:00}" },
                        { "OriginalId", "" }
                    },
                    false));


            // Add new (empty) ContentType for Settings
            if (appSettings == null)
                addList.Add(new AddContentTypeAndOrEntityTask(AppLoadConstants.TypeAppSettings));

            // add new (empty) ContentType for Resources
            if (appResources == null)
                addList.Add(new AddContentTypeAndOrEntityTask(AppLoadConstants.TypeAppResources));

            if (CreateAllMissingContentTypes(appState, addList))
            {
                SystemManager.Purge(appState);
                // get the latest app-state, but not-initialized so we can make changes
                var repoLoader = _repositoryLoaderGenerator.New();
                appState = repoLoader.AppState(appState.AppId, false);
                //_appManager = null; // reset, because afterwards we need a clean AppManager
            }

            addList.ForEach(task => MetadataEnsureTypeAndSingleEntity(appState, task));

            // Reset App-State to ensure it's reloaded with the added configuration
            SystemManager.Purge(appState);

            return (false, "ok");
        });

        private static string PickCorrectFolderName(string newAppName, string eavAppName)
        {
            if (eavAppName == Constants.DefaultAppGuid)
                return Constants.ContentAppFolder;
            if (eavAppName == Constants.PrimaryAppGuid || eavAppName == Constants.PrimaryAppName)
                return Constants.PrimaryAppName;
            return string.IsNullOrEmpty(newAppName)
                ? eavAppName
                : RemoveIllegalCharsFromPath(newAppName);
        }


        private bool CreateAllMissingContentTypes(AppState appState, List<AddContentTypeAndOrEntityTask> newItems) => Log.Func($"Check for {newItems.Count}", l =>
        {
            var appManager = _appManagerGenerator.New().InitWithState(appState, true);
            var addedTypes = false;
            foreach (var item in newItems)
                if (item.InAppType && FindContentType(appState, item.SetName, item.InAppType) == null)
                {
                    l.A("couldn't find type, will create");
                    // create App-Man if not created yet
                    appManager.ContentTypes.Create(item.SetName, Scopes.App);
                    addedTypes = true;
                }
                else
                    l.A($"Type '{item.SetName}' found");

            return addedTypes;
        });
        
        private void MetadataEnsureTypeAndSingleEntity(AppState appState,
            AddContentTypeAndOrEntityTask cTypeAndOrEntity) => Log.Do($"{cTypeAndOrEntity.SetName} for app {appState.AppId} - inApp: {cTypeAndOrEntity.InAppType}", l =>
        {
            var ct = FindContentType(appState, cTypeAndOrEntity.SetName, cTypeAndOrEntity.InAppType);

            // if it's still null, we have a problem...
            if (ct == null)
            {
                l.A("type is still null, error");
                throw new Exception("something went wrong - can't find type in app, but it's not a global type, so I must cancel");
            }

            var values = cTypeAndOrEntity.Values ?? new Dictionary<string, object>();
            var mdTarget = new Target((int)TargetTypes.App, "App", keyNumber: appState.AppId);
            var newEnt = _builder.Value.Entity.Create(appId: appState.AppId, guid: Guid.NewGuid(),
                contentType: ct,
                attributes: _builder.Value.Attribute.Create(values), metadataFor: mdTarget);
            //newEnt.SetMetadata(new Target((int)TargetTypes.App, null) { KeyNumber = AppState.AppId });

            // #ExtractEntitySave - verified
            //var appManager = _appManagerGenerator.New().InitWithState(appState, true);
            //appManager.Entities.Save(newEnt);
            _appWork.EntitySave(appState).Save(newEnt);
        });

        private IContentType FindContentType(AppState appState, string setName, bool inAppType)
        {
            // if it's an in-app type, it should check the app, otherwise it should check the global type
            // we're NOT asking the app for all types (which would be the normal way)
            // because there are rare cases where historic data accidentally
            // created the 2SexyContent-App type as a local type in an app (2sxc 9.20-9.22)
            // Basically after this update has run for a while - probably till end of 2018-04
            // this is probably not so important any more, but I would leave it forever for now
            // discuss w/2dm if you think you want to change this
            var ct = inAppType
                ? appState.GetContentType(setName)
                : _appStates.GetPresetApp().GetContentType(setName);
            return ct;
        }

        private static string RemoveIllegalCharsFromPath(string path)
        {
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex($"[{Regex.Escape(regexSearch)}]");
            return r.Replace(path, "");
        }


        private class AddContentTypeAndOrEntityTask
        {
            public readonly string SetName;
            public readonly Dictionary<string, object> Values;
            public readonly bool InAppType;

            public AddContentTypeAndOrEntityTask(string setName, Dictionary<string, object> values = null, bool inAppType = true)
            {
                SetName = setName;
                Values = values;
                InAppType = inAppType;
            }
        }
    }
}
