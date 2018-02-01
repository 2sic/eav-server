using System;
using System.Linq;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.ValueProvider;

namespace ToSic.Eav.Apps
{
    public partial class App: HasLog, IApp
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

        
    }
}
