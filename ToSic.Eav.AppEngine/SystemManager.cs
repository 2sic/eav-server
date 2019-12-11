using System;
using System.Collections.Generic;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps
{
    public class SystemManager: HasLog
    {
        #region Constructor

        public SystemManager(ILog parentLog = null) : base("App.SysMng", parentLog) { }

        #endregion



        #region purge cache stuff


        /// <summary>
        /// Flush the entire map of zones / apps
        /// </summary>
        public static void PurgeZoneList() => Purge(0,0,true);

        /// <summary>
        /// Clear the cache of a specific app/zone, or of everything
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        /// <param name="global">if true, will flush everything</param>
        public static void Purge(int zoneId, int appId, bool global = false)
        {
            if (global)
                DataSource.GetCache(null).PurgeGlobalCache();
            else
                DataSource.GetCache(null).PurgeCache(zoneId, appId);
        }

        /// <summary>
        /// Purge the cache of one app
        /// </summary>
        /// <param name="appId"></param>
        public static void Purge(int appId) => Purge(SystemRuntime.ZoneIdOfApp(appId), appId);

        public void InformOfPartialUpdate(AppIdentity app, IEnumerable<int> entities)
        {
            // var zoneId = SystemRuntime.ZoneIdOfApp(app.ZoneId, app.AppId);
            var cache = DataSource.GetCache(app.ZoneId, app.AppId);
            cache.LinkLog(Log);
            cache.PartialUpdate(entities);
        }

        /// <summary>
        /// Run some code and then purge the cache after that for full rebuild
        /// </summary>
        public static void DoAndPurge(int zoneId, int appId, Action action, bool global = false)
        {
            action.Invoke();
            Purge(zoneId, appId, global);
        }
        #endregion

    }
}
