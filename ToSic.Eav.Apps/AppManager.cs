using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Types;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// The app management system - it's meant for modifying the app, not for reading the configuration. 
    /// Use other mechanisms if you only want to read content-types etc.
    /// </summary>
    public class AppManager: AppRuntimeBase<AppManager>
    {
        public IServiceProvider ServiceProvider { get; }

        #region Constructors

        public AppManager(IServiceProvider serviceProvider, DataSourceFactory dataSourceFactory) : base(dataSourceFactory, "Eav.AppMan")
        {
            ServiceProvider = serviceProvider;
        }
        protected AppManager(IServiceProvider serviceProvider, DataSourceFactory dataSourceFactory, string logName) : base(dataSourceFactory, logName)
        {
            ServiceProvider = serviceProvider;
        }

        public AppManager Init(IAppIdentity app, ILog parentLog)
        {
            Init(app, true, parentLog);
            return this;
        }

        public AppManager Init(int appId, ILog parentLog) => Init(State.Identity(null, appId), true, parentLog);

        #endregion

        #region Access the Runtime
        /// <summary>
        /// Read / Runtime system of the AppManager, to read data
        /// </summary>
        public AppRuntime Read => _runtime ?? (_runtime = Factory.Resolve<AppRuntime>().Init(this, ShowDrafts, Log));
        private AppRuntime _runtime;
        #endregion

        /// <summary>
        /// Database controller / DB-Context
        /// </summary>
        internal DbDataController DataController 
            => _eavContext ?? (_eavContext = DbDataController.Instance(ZoneId, AppId, Log));
        private DbDataController _eavContext;

        /// <summary>
        /// Storage system providing another interface
        /// </summary>
        internal IStorage Storage => DataController;

        /// <summary>
        /// The entity-management subsystem
        /// </summary>
        public EntitiesManager Entities => _entities ?? (_entities = Factory.Resolve<EntitiesManager>().Init(this, Log));
        private EntitiesManager _entities;

        /// <summary>
        /// Queries Management Subsystem
        /// </summary>
        public QueryManager Queries => _queries ?? (_queries = new QueryManager().Init(this, Log));
        private QueryManager _queries;

        /// <summary>
        /// Content-Types Manager Subsystem
        /// </summary>
        public ContentTypeManager ContentTypes => _contentTypes ?? (_contentTypes = new ContentTypeManager().Init(this, Log));
        private ContentTypeManager  _contentTypes;



        private void MetadataEnsureTypeAndSingleEntity(string scope, string setName, string label, int appAssignment, Dictionary<string, object> values, bool inAppType)
        {
            var wrapLog = Log.Call($"{scope}/{setName} and {label} for app {AppId} MetadataAssignment: {appAssignment} - inApp: {inAppType}");

            // if it's an in-app type, it should check the app, otherwise it should check the global type
            // this is important, because there are rare cases where historic data accidentally
            // created the 2SexyContent-App type as a local type in an app (2sxc 9.20-9.22)
            // Basically after this update has run for a while - probably till end of 2018-04
            // this is probably not so important any more, but I would leave it forever for now
            // discuss w/2dm if you think you want to change this
            var ct = inAppType 
                ? Read.ContentTypes.Get(setName)
                : Global.FindContentType(setName);

            if (ct == null && inAppType)
            {
                Log.Add("couldn't find type, will create");
                ContentTypes.Create(setName, setName, label, scope);
                ct = Read.ContentTypes.Get(setName);
            }
            else
                Log.Add($"Type '{setName}' found");

            // if it's still null, we have a problem...
            if (ct == null)
            {
                Log.Add("type is still null, error");
                wrapLog("error");
                throw new Exception("something went wrong - can't find type in app, but it's not a global type, so I must cancel");
            }

            values = values ?? new Dictionary<string, object>();

            var newEnt = new Entity(AppId, Guid.NewGuid(), ct, values);
            newEnt.SetMetadata(new Metadata.Target { KeyNumber = AppId, TargetType = appAssignment });
            Entities.Save(newEnt);

            SystemManager.Purge(ZoneId, AppId, log: Log);
            wrapLog(null);
        }


        /// <summary>
        /// Will create a new app in the system and initialize the basic settings incl. the 
        /// app-definition
        /// </summary>
        /// <returns></returns>
        public static void AddBrandNewApp(int zoneId, string appName, ILog parentLog)
        {
            // check if invalid app-name
            if (appName == Constants.ContentAppName || appName == Constants.DefaultAppName || string.IsNullOrEmpty(appName) || !Regex.IsMatch(appName, "^[0-9A-Za-z -_]+$"))
                throw new ArgumentOutOfRangeException("appName '" + appName + "' not allowed");

            var appId = new ZoneManager().Init(zoneId, parentLog).CreateApp();
            // TODO: #DI
            Factory.Resolve<AppManager>().Init(new AppIdentity(zoneId, appId), parentLog)
                .EnsureAppIsConfigured(appName);
        }

        /// <summary>
        /// Create app-describing entity for configuration and add Settings and Resources Content Type
        /// </summary>
        public void EnsureAppIsConfigured(string appName = null)
        {
            var wrapLog = Log.Call($"{nameof(appName)}: {appName}");

            var mds = State.Get(this);
            var appConfig = mds.Get(Constants.MetadataForApp, AppId, AppConstants.TypeAppConfig).FirstOrDefault();
            var appResources = mds.Get(Constants.MetadataForApp, AppId, AppConstants.TypeAppResources).FirstOrDefault();
            var appSettings = mds.Get(Constants.MetadataForApp, AppId, AppConstants.TypeAppSettings).FirstOrDefault();

            Log.Add($"App Config: {appConfig != null}, Resources: {appResources != null}, Settings: {appSettings != null}");

            // if nothing must be done, return now
            if (appConfig != null && appResources != null && appSettings != null)
            {
                wrapLog("ok");
                return;
            }

            // Get appName from cache - stop if it's a "Default" app
            var eavAppName = new ZoneRuntime().Init(ZoneId, Log).GetName(AppId);

            // v10.25 from now on the DefaultApp can also have settings and resources
            //if (eavAppName == Constants.DefaultAppName)
            //    return;
            var folder = eavAppName == Constants.DefaultAppName
                ? Constants.ContentAppFolder
                : string.IsNullOrEmpty(appName)
                    ? eavAppName
                    : string.IsNullOrEmpty(appName) ? eavAppName : RemoveIllegalCharsFromPath(appName);

            //var appMan = new AppManager(appIdentity, Log);
            if (appConfig == null)
                MetadataEnsureTypeAndSingleEntity(AppConstants.ScopeApp,
                    AppConstants.TypeAppConfig,
                    "App Metadata",
                    Constants.MetadataForApp,
                    new Dictionary<string, object>
                    {
                        {"DisplayName", string.IsNullOrEmpty(appName) ? eavAppName : appName},
                        {"Folder", folder},
                        {"AllowTokenTemplates", "True"},
                        {"AllowRazorTemplates", "True"},
                        {"Version", "00.00.01"},
                        {"OriginalId", ""}
                    }, 
                    false);


            // Add new (empty) ContentType for Settings
            if (appSettings == null)
                MetadataEnsureTypeAndSingleEntity(AppConstants.ScopeApp,
                    AppConstants.TypeAppSettings,
                    "Stores settings for an app",
                    Constants.MetadataForApp,
                    null,
                    true);

            // add new (empty) ContentType for Resources
            if (appResources == null)
                MetadataEnsureTypeAndSingleEntity(AppConstants.ScopeApp,
                    AppConstants.TypeAppResources,
                    "Stores resources like translations for an app",
                    Constants.MetadataForApp,
                    null, 
                    true);

            //if (appConfig == null || appSettings == null || appResources == null)
            SystemManager.Purge(ZoneId, AppId, log: Log);

            wrapLog("ok");
        }


        private static string RemoveIllegalCharsFromPath(string path)
        {
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex($"[{Regex.Escape(regexSearch)}]");
            return r.Replace(path, "");
        }
    }
}
