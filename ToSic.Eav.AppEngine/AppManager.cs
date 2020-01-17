using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
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
    public class AppManager: AppRuntimeBase
    {
        #region Constructors
        public AppManager(IAppIdentity app, ILog parentLog) : base(app, true, parentLog) { RenameLog();}

        public AppManager(int appId, ILog parentLog) 
            : this(State.Identity(null, appId:appId), parentLog) { RenameLog();}

        private void RenameLog() => Log.Rename("AppMan");

        #endregion

        #region Access the Runtime

        public AppRuntime Read => _runtime ?? (_runtime = new AppRuntime(Data, ShowDrafts, Log));
        private AppRuntime _runtime;
        #endregion

        internal DbDataController DataController 
            => _eavContext ?? (_eavContext = DbDataController.Instance(ZoneId, AppId, Log));
        private DbDataController _eavContext;


        internal IStorage Storage => DataController;

        /// <summary>
        /// The entity-management subsystem
        /// </summary>
        public EntitiesManager Entities => _entities ?? (_entities = new EntitiesManager(this, Log));
        private EntitiesManager _entities;

        public QueryManager Queries => _queries ?? (_queries = new QueryManager(this, Log));
        private QueryManager _queries;

        public ContentTypeManager ContentTypes => _contentTypes ?? (_contentTypes = new ContentTypeManager(this, Log));
        private ContentTypeManager  _contentTypes;



        public void MetadataEnsureTypeAndSingleEntity(string scope, string setName, string label, int appAssignment, Dictionary<string, object> values, bool inAppType)
        {
            var wrapLog = Log.Call($"{scope}/{setName} and {label} for app {AppId} MetadataAssignment: {appAssignment} - inApp: {inAppType}");
            //Log.Add($"check / create for {scope}/{setName} and {label} for app {appAssignment} - inApp: {inAppType}");

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
            newEnt.SetMetadata(new Metadata.Target { KeyNumber = /*DataController.*/AppId, TargetType = appAssignment });
            Entities.Save(newEnt);

            SystemManager.Purge(ZoneId, AppId);
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

            var appId = new ZoneManager(zoneId, parentLog).CreateApp();
            new AppManager(new AppIdentity(zoneId, appId), parentLog)
                .EnsureAppIsConfigured( /*zoneId, appId, parentLog, */appName);
        }

        /// <summary>
        /// Create app-describing entity for configuration and add Settings and Resources Content Type
        /// </summary>
        public /*static*/ void EnsureAppIsConfigured(/*int zoneId, int appId, ILog parentLog,*/ string appName = null)
        {
            //var log = new Log("Eav.AppMan", parentLog);
            var wrapLog = Log.Call($"{nameof(appName)}: {appName}");

            //var appIdentity = new AppIdentity(ZoneId, AppId);
            var mds = State.Get(/*appIdentity*/this);
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
            var eavAppName = new ZoneRuntime(ZoneId, /*parentLog*/Log).GetName(AppId);

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
                /*appMan.*/MetadataEnsureTypeAndSingleEntity(AppConstants.ScopeApp,
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
                /*appMan.*/MetadataEnsureTypeAndSingleEntity(AppConstants.ScopeApp,
                    AppConstants.TypeAppSettings,
                    "Stores settings for an app",
                    Constants.MetadataForApp,
                    null,
                    true);

            // add new (empty) ContentType for Resources
            if (appResources == null)
                /*appMan.*/MetadataEnsureTypeAndSingleEntity(AppConstants.ScopeApp,
                    AppConstants.TypeAppResources,
                    "Stores resources like translations for an app",
                    Constants.MetadataForApp,
                    null, 
                    true);

            //if (appConfig == null || appSettings == null || appResources == null)
            SystemManager.Purge(ZoneId, AppId);

            wrapLog("ok");
        }

        //private IEntity AppSettingsDummy() => AppMetaDummy(AppConstants.TypeAppSettings, new Dictionary<string, object>());

        //private IEntity AppMetaDummy(string typeName, Dictionary<string, object> values)
        //{
        //    var ct = Read.ContentTypes.Get(typeName);
        //    return new Entity(AppId, Guid.NewGuid(), ct, values);
        //}

        private static string RemoveIllegalCharsFromPath(string path)
        {
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex($"[{Regex.Escape(regexSearch)}]");
            return r.Replace(path, "");
        }
    }
}
