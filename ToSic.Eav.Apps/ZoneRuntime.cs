using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps
{
    public class ZoneRuntime: ZoneBase
    {
        #region Constructor and simple properties

        public ZoneRuntime(int zoneId, ILog parentLog) : base(zoneId, parentLog, "App.Zone") {}

        #endregion


        public int DefaultAppId => Cache.Zones[ZoneId].DefaultAppId;

        public Dictionary<int, string> Apps => Cache.Zones[ZoneId].Apps;

        public string GetName(int appId) => Cache.Zones[ZoneId].Apps[appId];

        public List<DimensionDefinition> Languages(bool includeInactive = false) => includeInactive 
            ? Cache.Zones[ZoneId].Languages 
            : Cache.Zones[ZoneId].Languages.Where(l => l.Active).ToList();

        /// <summary>
        /// Find the app id from the app-name (usually a guid or "Default").
        /// Can also check the folder name
        /// </summary>
        /// <returns></returns>
        public int FindAppId(string appName, bool alsoCheckFolderName = false)
        {
            var zones = State.Zones;

            if (string.IsNullOrEmpty(appName))
                return AppConstants.AppIdNotFound;

            var appId = zones[ZoneId].Apps
                .Where(p => p.Value == appName)
                .Select(p => p.Key).FirstOrDefault();

            // optionally check folder names
            if (appId == 0 && alsoCheckFolderName)
                appId = AppIdFromFolderName(appName);

            return appId > 0 ? appId : AppConstants.AppIdNotFound;
        }

        /// <summary>
        /// Find an app based on the folder name. Will check the App Metadata for this
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        private int AppIdFromFolderName(string folderName)
        {
            var nameLower = folderName.ToLower();

            foreach (var p in State.Zones[ZoneId].Apps)
            {
                var mds = State.Get(new AppIdentity(ZoneId, p.Key));
                var appMetaData = mds
                    .Get(SystemRuntime.MetadataType(Constants.AppAssignmentName), p.Key,
                        AppConstants.TypeAppConfig)
                    .FirstOrDefault();
                var folder = appMetaData?.GetBestValue(AppConstants.FieldFolder).ToString();
                if (!string.IsNullOrEmpty(folder) && folder.ToLower() == nameLower)
                    return p.Key;
            }

            // not found
            return AppConstants.AppIdNotFound;
        }

    }
}
