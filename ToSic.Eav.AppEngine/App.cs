using System;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// A populated app-object providing quick simple api to access
    /// name, folder, data, metadata etc.
    /// </summary>
    public partial class App: AppIdentity, IApp
    {
        public const int AutoLookupZone = -1;

        public string Name { get; private set; }
        public string Folder { get; private set; }
        public bool Hidden { get; private set; }

        public string AppGuid { get; }

        public bool ShowDrafts { get; private set; }
        public bool EnablePublishing { get; private set; }

        protected const string IconFile = "/" + AppConstants.AppIconFile;

        internal App(int zoneId, 
            int appId, 
            bool allowSideEffects,
            Func<App, IAppDataConfiguration> buildConfiguration,
            ILog parentLog, 
            string logMsg)
            // first, initialize the AppIdentity and log it's use
            : base(zoneId, appId, parentLog, "App.2sxcAp", $"prep App z#{zoneId}, a#{appId}, allowSE:{allowSideEffects}, hasDataConfig:{buildConfiguration != null}, {logMsg}")
        {
            // if zone is missing, try to find it; if still missing, throw error
            if (zoneId == AutoLookupZone) throw new Exception("Cannot find zone-id for portal specified");

            // Look up name in cache
            var cache = (BaseCache) DataSource.GetCache(zoneId, appId);
            AppDataPackage = cache.AppDataPackage; // for metadata

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

            // for deferred initialization as needed
            _dataConfigurationBuilder = buildConfiguration;
        }
    }
}
