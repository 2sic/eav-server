using System;
using ToSic.Eav.Caching;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps
{
    // Todo: Probably rename since it's only job is to purge - maybe AppStatePurger
    public class SystemManager: HasLog<SystemManager>
    {
        #region Constructor

        public SystemManager(IAppStates appStates, IAppsCache appsCache): base("App.SysMng")
        {
            _appStates = appStates;
            _appsCache = appsCache;
        }
        private readonly IAppStates _appStates;
        private readonly IAppsCache _appsCache;

        #endregion



        #region purge cache stuff

        /// <summary>
        /// Flush the entire map of zones / apps
        /// </summary>
        public void PurgeZoneList() => Purge(new AppIdentity(0, 0),true);

        /// <summary>
        /// Clear the cache of a specific app/zone, or of everything
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        /// <param name="global">if true, will flush everything</param>
        public void Purge(int zoneId, int appId, bool global = false) 
            => Purge(new AppIdentity(zoneId, appId), global);

        /// <summary>
        /// Clear the cache of a specific app/zone, or of everything
        /// </summary>
        /// <param name="appIdentity"></param>
        /// <param name="global">if true, will flush everything</param>
        public void Purge(IAppIdentity appIdentity, bool global = false)
        {
            var wrapLog = Log.Call($"{appIdentity.Show()}, {global}");
            if (global)
                _appsCache.PurgeZones();
            else
                _appsCache.Purge(appIdentity);
            wrapLog.Invoke("ok");
        }

        ///// <summary>
        ///// Purge the cache of one app
        ///// </summary>
        ///// <param name="appId"></param>
        ///// <param name="log">log which will then log that it purged this</param>
        //public static void Purge(int appId, ILog log)
        //{
        //    var wrapLog = log?.Call($"{appId}");
        //    Purge(SystemRuntime.ZoneIdOfApp(appId), appId, log: log);
        //    wrapLog?.Invoke("ok");
        //}

        /// <summary>
        /// Purge the cache of one app
        /// </summary>
        /// <param name="appId"></param>
        public void PurgeApp(int appId)
        {
            var wrapLog = Log.Call($"{appId}");
            Purge(_appStates.IdentityOfApp(appId));
            wrapLog.Invoke("ok");
        }

        /// <summary>
        /// Run some code and then purge the cache after that for full rebuild
        /// </summary>
        public void DoAndPurge(int zoneId, int appId, Action action, bool global = false)
        {
            var wrapLog = Log.Call($"{zoneId}, {appId}, fn(...), {global}");
            action.Invoke();
            Purge(zoneId, appId, global);
            wrapLog.Invoke("ok");
        }
        #endregion

    }
}
