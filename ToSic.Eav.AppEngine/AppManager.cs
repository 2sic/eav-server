using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// The app management system - it's meant for modifying the app, not for reading the configuration. 
    /// Use other mechanisms if you only want to read content-types etc.
    /// </summary>
    public class AppManager: AppBase
    {
        #region Constructors
        public AppManager(int zoneId, int appId, Log parentLog = null) : base(zoneId, appId, parentLog) { RenameLog();}

        public AppManager(IAppIdentity app, Log parentLog) : base(app, parentLog) { RenameLog();}
        public AppManager(int appId, Log parentLog) : base(appId, parentLog) { RenameLog();}

        private void RenameLog() => Log.Rename("AppMan");

        #endregion

        #region Access the Runtime

        public AppRuntime Read => _runtime ?? (_runtime = new AppRuntime(Data, Log));
        private AppRuntime _runtime;
        #endregion

        internal DbDataController DataController 
            => _eavContext ?? (_eavContext = DbDataController.Instance(ZoneId, AppId, Log));
        private DbDataController _eavContext;


        internal IStorage Storage => DataController;

        /// <summary>
        /// The template management subsystem
        /// </summary>
        public TemplatesManager Templates => _templates ?? (_templates = new TemplatesManager(this, Log));
        private TemplatesManager _templates;

        /// <summary>
        /// The entity-management subsystem
        /// </summary>
        public EntitiesManager Entities => _entities ?? (_entities = new EntitiesManager(this, Log));
        private EntitiesManager _entities;

        public QueryManager Queries => _queries ?? (_queries = new QueryManager(this, Log));
        private QueryManager _queries;

        public ContentTypeManager ContentTypes => _contentTypes ?? (_contentTypes = new ContentTypeManager(this, Log));
        private ContentTypeManager  _contentTypes;



        public void MetadataEnsureTypeAndSingleEntity(string scope, string setName, string label, int appAssignment, Dictionary<string, object> values)
        {
            //ToSicEavAttributeSets contentType;
            if (!DataController.AttribSet.DbAttribSetExists(AppId, setName))
            {
                //contentType = 
                DataController.AttribSet.PrepareDbAttribSet(setName, label, setName, scope, false, null);
                DataController.SqlDb.SaveChanges();
            }
            //else
            //{
            //    contentType = DataController.AttribSet.GetDbAttribSet(setName);
            //}

            if (values == null)
                values = new Dictionary<string, object>();

            var newEnt = new Entity(AppId, 0, setName, values);
            newEnt.SetMetadata(new MetadataFor { KeyNumber = DataController.AppId, TargetType = appAssignment });
            Entities.Save(newEnt);

            SystemManager.Purge(ZoneId, AppId);
        }


        /// <summary>
        /// Will create a new app in the system and initialize the basic settings incl. the 
        /// app-definition
        /// </summary>
        /// <returns></returns>
        public static void AddBrandNewApp(int zoneId, string appName, Log parentLog)
        {
            // check if invalid app-name
            if (appName == Constants.ContentAppName || appName == Constants.DefaultAppName || string.IsNullOrEmpty(appName) || !Regex.IsMatch(appName, "^[0-9A-Za-z -_]+$"))
                throw new ArgumentOutOfRangeException("appName '" + appName + "' not allowed");

            var appId = new ZoneManager(zoneId, parentLog).CreateApp();
            EnsureAppIsConfigured(zoneId, appId, parentLog, appName);
        }

        /// <summary>
        /// Create app-describing entity for configuration and add Settings and Resources Content Type
        /// </summary>
        internal static void EnsureAppIsConfigured(int zoneId, int appId, Log parentLog, string appName = null)
        {
            var appAssignment = SystemRuntime.MetadataType(Constants.AppAssignmentName);
            var scope = AppConstants.AttributeSetScopeApps;
            var mds = DataSource.GetMetaDataSource(zoneId, appId);
            var appMetaData = mds.GetMetadata(appAssignment, appId, AppConstants.AttributeSetStaticNameApps).FirstOrDefault();
            var appResources = mds.GetMetadata(appAssignment, appId, AppConstants.AttributeSetStaticNameAppResources).FirstOrDefault();
            var appSettings = mds.GetMetadata(appAssignment, appId, AppConstants.AttributeSetStaticNameAppSettings).FirstOrDefault();

            // Get appName from cache - stop if it's a "Default" app
            var eavAppName = new ZoneRuntime(zoneId, parentLog).GetName(appId);

            if (eavAppName == Constants.DefaultAppName)
                return;

            var appMan = new AppManager(zoneId, appId);
            if (appMetaData == null)
                appMan.MetadataEnsureTypeAndSingleEntity(scope,
                    AppConstants.AttributeSetStaticNameApps,
                    "App Metadata",
                    appAssignment,
                    new Dictionary<string, object>
                    {
                        {"DisplayName", string.IsNullOrEmpty(appName) ? eavAppName : appName},
                        {"Folder", string.IsNullOrEmpty(appName) ? eavAppName : RemoveIllegalCharsFromPath(appName)},
                        {"AllowTokenTemplates", "False"},
                        {"AllowRazorTemplates", "False"},
                        {"Version", "00.00.01"},
                        {"OriginalId", ""}
                    });


            // Add new (empty) ContentType for Settings
            if (appSettings == null)
                appMan.MetadataEnsureTypeAndSingleEntity(scope,
                    AppConstants.AttributeSetStaticNameAppSettings,
                    "Stores settings for an app",
                    appAssignment,
                    null);

            // add new (empty) ContentType for Resources
            if (appResources == null)
                appMan.MetadataEnsureTypeAndSingleEntity(scope,
                    AppConstants.AttributeSetStaticNameAppResources,
                    "Stores resources like translations for an app",
                    appAssignment,
                    null);

            if (appMetaData == null || appSettings == null || appResources == null)
                SystemManager.Purge(zoneId, appId);
        }

        private static string RemoveIllegalCharsFromPath(string path)
        {
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex($"[{Regex.Escape(regexSearch)}]");
            return r.Replace(path, "");
        }
    }
}
