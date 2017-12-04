using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.DataSources.Pipeline;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.ValueProvider;

namespace ToSic.Eav.Apps
{
    public class App: HasLog, IApp
    {
        protected const int AutoLookup = -1;

        public int AppId { get; }
        public int ZoneId { get; }

        public string Name { get; private set; }
        public string Folder { get; private set; }
        public bool Hidden { get; private set; }

        public string AppGuid { get; set; }


        protected IValueCollectionProvider ConfigurationProvider { get; set; }
        protected bool ShowDraftsInData { get; set; }
        protected bool VersioningEnabled { get; set; }

        protected const string IconFile = "/" + AppConstants.AppIconFile;
        protected const string ContentAppName = Constants.ContentAppName;






        /// <summary>
        /// needed to initialize data - must always happen a bit later because the show-draft info isn't available when creating the first App-object.
        /// todo: later this should be moved to initialization of this object
        /// </summary>
        /// <param name="showDrafts"></param>
        /// <param name="versioningEnabled"></param>
        /// <param name="configurationValues">this is needed for providing parameters to the data-query-system</param>
        public void InitData(bool showDrafts, bool versioningEnabled, IValueCollectionProvider configurationValues)
        {
            Log.Add($"init data drafts:{showDrafts}, vers:{versioningEnabled}, hasConf:{configurationValues != null}");
            ConfigurationProvider = configurationValues;
            ShowDraftsInData = showDrafts;
            VersioningEnabled = versioningEnabled;
        }


        internal App(int zoneId, int appId, bool allowSideEffects, Log parentLog, string logMsg)
            : base("App.2sxcAp", parentLog, $"prep App z#{zoneId}, a#{appId}, allowSE:{allowSideEffects}, {logMsg}")
        {
            // if zone is missing, try to find it; if still missing, throw error
            if (zoneId == AutoLookup) throw new Exception("Cannot find zone-id for portal specified");

            // provide basic values
            AppId = appId;
            ZoneId = zoneId;

            // Look up name in cache
            AppGuid = ((BaseCache)DataSource.GetCache(zoneId)).ZoneApps[zoneId].Apps[appId];

            if (AppGuid == Constants.DefaultAppName)
                Name = Folder = ContentAppName;
            else
            {
                // if it's a real App (not content/default), do more
                Log.Add($"create app resources? allowSE:{allowSideEffects}");

                if (allowSideEffects)
                    AppManager.EnsureAppIsConfigured(ZoneId, AppId, Log); // make sure additional settings etc. exist
                InitializeResourcesSettingsAndMetadata();
            }
        }


        #region Settings, Config, Metadata
        protected IEntity AppMetadata;
        protected IEntity AppSettings;
        protected IEntity AppResources;

        /// <summary>
        /// Assign all kinds of metadata / resources / settings (App-Mode only)
        /// </summary>
        protected void InitializeResourcesSettingsAndMetadata()
        {
            Log.Add("init app resources");

            // Get app-describing entity
            var appAssignmentId = SystemRuntime.MetadataType(Constants.AppAssignmentName);
            var mds = DataSource.GetMetaDataSource(ZoneId, AppId);
            AppMetadata = mds.GetMetadata(appAssignmentId, AppId,
                        AppConstants.AttributeSetStaticNameApps)
                    .FirstOrDefault();

            Name = AppMetadata?.GetBestValue("DisplayName")?.ToString() ?? "Error";
            Folder = AppMetadata?.GetBestValue("Folder")?.ToString() ?? "Error";
            if (Boolean.TryParse(AppMetadata?.GetBestValue("Hidden")?.ToString(), out bool hidden))
                Hidden = hidden;

            AppResources = mds.GetMetadata(appAssignmentId, AppId,
                        AppConstants.AttributeSetStaticNameAppResources)
                    .FirstOrDefault();

            AppSettings = mds.GetMetadata(appAssignmentId, AppId,
                        AppConstants.AttributeSetStaticNameAppSettings)
                    .FirstOrDefault();
        }
        #endregion

        #region Data

        public IAppData Data => _data ?? (_data = BuildData());
        private IAppData _data;

        protected virtual DataSources.App BuildData()
        {
            Log.Add("configure on demand start");
            if (ConfigurationProvider == null)
                throw new Exception("Cannot provide Data for the object App as crucial information is missing. " +
                                    "Please call InitData first to provide this data.");

            // ModulePermissionController does not work when indexing, return false for search
            var initialSource = DataSource.GetInitialDataSource(ZoneId, AppId, ShowDraftsInData, 
                ConfigurationProvider as ValueCollectionProvider, Log);

            // todo: probably use the full configuration provider from function params, not from initial source?
            var xData = DataSource.GetDataSource<DataSources.App>(initialSource.ZoneId,
                initialSource.AppId, initialSource, initialSource.ConfigurationProvider, Log);

            Log.Add("configure on demand completed");
            return xData;
        }


        #endregion


        #region query stuff
        /// <summary>
        /// Cached list of queries
        /// </summary>
        protected IDictionary<string, IDataSource> Queries;

        /// <summary>
        /// Accessor to queries. Use like:
        /// - App.Query.Count
        /// - App.Query.ContainsKey(...)
        /// - App.Query["One Event"].List
        /// </summary>
        public IDictionary<string, IDataSource> Query
        {
            get
            {
                if (Queries != null) return Queries;

                if (ConfigurationProvider == null)
                    throw new Exception("Can't use app-queries, because the necessary configuration provider hasn't been initialized. Call InitData first.");
                Queries = DataPipeline.AllPipelines(ZoneId, AppId, ConfigurationProvider, Log);
                return Queries;
            }
        }

        public DeferredPipelineQuery GlobalQueryBeta(string name)
        {
            var qent = Eav.DataSources.Queries.Global.FindQuery(name);
            if (qent == null)
                throw new Exception($"can't find (BETA) global query {name}");
            var query = new DeferredPipelineQuery(AppId, ZoneId, qent, ConfigurationProvider);
            return query;
        }

        #endregion 
    }
}
