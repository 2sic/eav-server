using System;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav.Apps
{
    public class SystemManager
    {

        #region purge cache stuff

        public static void PurgeZoneList() => Purge(0,0,true);

        public static void Purge(int zoneId, int appId, bool global = false)
        {
            if (global)
                DataSource.GetCache(null).PurgeGlobalCache();
            else
                DataSource.GetCache(null).PurgeCache(zoneId, appId);
        }

        public static void Purge(int appId) => Purge(SystemRuntime.ZoneIdOfApp(appId), appId);

        //public static int ZoneIdOfApp(int appId) 
        //    => ((BaseCache) DataSource.GetCache(null)).GetZoneAppId(appId: appId).Item1;

        public static void DoAndPurge(int zoneId, int appId, Action action, bool global = false)
        {
            action.Invoke();
            Purge(zoneId, appId, global);
        }
        #endregion

    }
}
