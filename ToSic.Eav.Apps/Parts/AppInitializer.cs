using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repositories;
using ToSic.Eav.Types;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// The AppInitializer is responsible for ensuring that an App-object has all the properties / metadata it needs. Specifically:
    /// - App Configuration (Folder, Version, etc.)
    /// - App Resources
    /// - App Settings
    /// It must be called from an AppManager, which has been created for this app
    /// </summary>
    public class AppInitializer : HasLog
    {
        #region Constructor / DI

        public AppInitializer(IServiceProvider serviceProvider, IGlobalTypes globalTypes, SystemManager systemManager) : base("Eav.AppBld")
        {
            _serviceProvider = serviceProvider;
            SystemManager = systemManager.Init(Log);
            _globalTypes = globalTypes;
        }
        private readonly IServiceProvider _serviceProvider;
        private readonly IGlobalTypes _globalTypes;
        protected readonly SystemManager SystemManager;


        public AppInitializer Init(AppState appState, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            AppState = appState;
            return this;
        }

        private AppState AppState { get; set; }

        /// <summary>
        /// The App Manager must be re-created during initialization
        /// So we don't inject into into this class, but instead create it on demand
        /// </summary>
        private AppManager AppManager =>
            _appManager ?? (_appManager = _serviceProvider.Build<AppManager>().InitWithState(AppState, true, Log));
        private AppManager _appManager;


        #endregion
        
        /// <summary>
        /// Create app-describing entity for configuration and add Settings and Resources Content Type
        /// </summary>
        /// <param name="newAppName">The app-name (for new apps) which would be the folder name as well. </param>
        public bool InitializeApp(string newAppName = null)
        {
            var wrapLog = Log.Call<bool>($"{nameof(newAppName)}: {newAppName}");

            if (AppInitializedChecker.CheckIfAllPartsExist(AppState, out var appConfig, out var appResources, out var appSettings, Log))
                return wrapLog("ok", true);

            // Get appName from cache - stop if it's a "Default" app
            var eavAppName = AppState.AppGuidName;

            // v10.25 from now on the DefaultApp can also have settings and resources
            var folder = eavAppName == Constants.DefaultAppName
                ? Constants.ContentAppFolder
                : string.IsNullOrEmpty(newAppName)
                    ? eavAppName
                    : string.IsNullOrEmpty(newAppName) ? eavAppName : RemoveIllegalCharsFromPath(newAppName);

            var addList = new List<AddItemTask>();
            if (appConfig == null)
                addList.Add(new AddItemTask(AppLoadConstants.TypeAppConfig,
                    "App Metadata",
                    new Dictionary<string, object>
                    {
                        {"DisplayName", string.IsNullOrEmpty(newAppName) ? eavAppName : newAppName},
                        {"Folder", folder},
                        {"AllowTokenTemplates", "True"},
                        {"AllowRazorTemplates", "True"},
                        {"Version", "00.00.11"}, // note: update this to the latest 2sxc version just so it's easy to spot when it was auto-created
                        {"OriginalId", ""}
                    },
                    false));


            // Add new (empty) ContentType for Settings
            if (appSettings == null)
                addList.Add(new AddItemTask(AppLoadConstants.TypeAppSettings,
                    "Stores settings for an app"));

            // add new (empty) ContentType for Resources
            if (appResources == null)
                addList.Add(new AddItemTask(AppLoadConstants.TypeAppResources,
                    "Stores resources like translations for an app"));

            if (CreateAllMissingContentTypes(addList))
            {
                SystemManager.Purge(AppState);
                // get the latest app-state, but not-initialized so we can make changes
                var repoLoader = _serviceProvider.Build<IRepositoryLoader>().Init(Log);
                AppState = repoLoader.AppState(AppState.AppId, false);
                _appManager = null; // reset, because afterwards we need a clean AppManager
            }

            addList.ForEach(MetadataEnsureTypeAndSingleEntity);

            // Reset App-State to ensure it's reloaded with the added configuration
            SystemManager.Purge(AppState);

            return wrapLog("ok", false);
        }



        private bool CreateAllMissingContentTypes(List<AddItemTask> newItems)
        {
            var wrapLog = Log.Call<bool>($"Check for {newItems.Count}");
            var addedTypes = false;
            foreach (var item in newItems)
                if (item.InAppType && FindContentType(item.SetName, item.InAppType) == null)
                {
                    Log.Add("couldn't find type, will create");
                    // create App-Man if not created yet
                    AppManager.ContentTypes.Create(item.SetName, item.SetName, item.Label, AppConstants.ScopeApp);
                    addedTypes = true;
                }
                else
                    Log.Add($"Type '{item.SetName}' found");

            return wrapLog("ok", addedTypes);
        }

        private void MetadataEnsureTypeAndSingleEntity(AddItemTask item)
        {
            var wrapLog = Log.Call($"{item.SetName} and {item.Label} for app {AppState.AppId} - inApp: {item.InAppType}");

            var ct = FindContentType(item.SetName, item.InAppType);

            // if it's still null, we have a problem...
            if (ct == null)
            {
                Log.Add("type is still null, error");
                wrapLog("error");
                throw new Exception("something went wrong - can't find type in app, but it's not a global type, so I must cancel");
            }

            var values = item.Values ?? new Dictionary<string, object>();

            var newEnt = new Entity(AppState.AppId, Guid.NewGuid(), ct, values);
            newEnt.SetMetadata(new Target((int)TargetTypes.App, null) { KeyNumber = AppState.AppId });
            AppManager.Entities.Save(newEnt);

            wrapLog(null);
        }

        private IContentType FindContentType(string setName, bool inAppType)
        {
            // if it's an in-app type, it should check the app, otherwise it should check the global type
            // we're NOT asking the app for all types (which would be the normal way)
            // because there are rare cases where historic data accidentally
            // created the 2SexyContent-App type as a local type in an app (2sxc 9.20-9.22)
            // Basically after this update has run for a while - probably till end of 2018-04
            // this is probably not so important any more, but I would leave it forever for now
            // discuss w/2dm if you think you want to change this
            var ct = inAppType
                ? AppManager.Read.ContentTypes.Get(setName)
                : _globalTypes.FindContentType(setName);
            return ct;
        }

        private static string RemoveIllegalCharsFromPath(string path)
        {
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex($"[{Regex.Escape(regexSearch)}]");
            return r.Replace(path, "");
        }


        private class AddItemTask
        {
            public readonly string SetName;
            public readonly string Label;
            public readonly Dictionary<string, object> Values;
            public readonly bool InAppType;

            public AddItemTask(string setName, string label, Dictionary<string, object> values = null, bool inAppType = true)
            {
                SetName = setName;
                Label = label;
                Values = values;
                InAppType = inAppType;
            }
        }
    }
}
