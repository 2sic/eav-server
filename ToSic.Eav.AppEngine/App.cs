using System;
using System.Linq;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.ValueProvider;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// A populated app-object providing quick simple api to access
    /// name, folder, data, metadata etc.
    /// </summary>
    public partial class App: AppIdentity, IApp
    {
        protected const int AutoLookup = -1;

        public string Name { get; private set; }
        public string Folder { get; private set; }
        public bool Hidden { get; private set; }

        public string AppGuid { get; }


        public IValueCollectionProvider ConfigurationProvider { get; private set; }
        protected bool ShowDraftsInData { get; private set; }
        protected bool VersioningEnabled { get; private set; }

        protected const string IconFile = "/" + AppConstants.AppIconFile;

        internal App(int zoneId, 
            int appId, 
            bool allowSideEffects,
            Func<App, IAppDataConfiguration> buildConfiguration,
            Log parentLog, 
            string logMsg)
            // first, initialize the AppIdentity
            : base(zoneId, appId, parentLog, "App.2sxcAp", $"prep App z#{zoneId}, a#{appId}, allowSE:{allowSideEffects}, {logMsg}")
        {
            // if zone is missing, try to find it; if still missing, throw error
            if (zoneId == AutoLookup) throw new Exception("Cannot find zone-id for portal specified");

            // Look up name in cache
            var cache = (BaseCache) DataSource.GetCache(zoneId, appId);
            _deferredLookupData = cache.AppDataPackage; // for metadata

            AppGuid = cache.ZoneApps[zoneId].Apps[appId];

            if (AppGuid == Constants.DefaultAppName)
                Name = Folder = Constants.ContentAppName;
            else
            {
                // if it's a real App (not content/default), do more
                Log.Add($"create app resources? allowSE:{allowSideEffects}");

                if (allowSideEffects)
                    AppManager.EnsureAppIsConfigured(ZoneId, AppId, Log); // make sure additional settings etc. exist
                InitializeResourcesSettingsAndMetadata();
            }

            InitData(buildConfiguration);
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
            if (bool.TryParse(AppMetadata?.GetBestValue("Hidden")?.ToString(), out var hidden))
                Hidden = hidden;

            AppResources = mds.GetMetadata(appAssignmentId, AppId,
                        AppConstants.AttributeSetStaticNameAppResources)
                    .FirstOrDefault();

            AppSettings = mds.GetMetadata(appAssignmentId, AppId,
                        AppConstants.AttributeSetStaticNameAppSettings)
                    .FirstOrDefault();
        }
        #endregion

        
    }
}
