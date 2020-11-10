using System;
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
        /// <param name="log">log which will then log that it purged this</param>
        public static void PurgeZoneList(ILog log) => Purge(0,0,true, log);

        /// <summary>
        /// Clear the cache of a specific app/zone, or of everything
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        /// <param name="global">if true, will flush everything</param>
        /// <param name="log">log which will then log that it purged this</param>
        public static void Purge(int zoneId, int appId, bool global = false, ILog log = null) 
            => Purge(new AppIdentity(zoneId, appId), global, log);

        /// <summary>
        /// Clear the cache of a specific app/zone, or of everything
        /// </summary>
        /// <param name="appIdentity"></param>
        /// <param name="global">if true, will flush everything</param>
        /// <param name="log">log which will then log that it purged this</param>
        public static void Purge(IAppIdentity appIdentity, bool global = false, ILog log = null)
        {
            var wrapLog = log?.Call($"{appIdentity.ZoneId}/{appIdentity.AppId}, {global}");
            if (global)
                State.Cache.PurgeZones();
            else
                State.Cache.Purge(appIdentity);
            wrapLog?.Invoke("ok");
        }

        /// <summary>
        /// Purge the cache of one app
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="log">log which will then log that it purged this</param>
        public static void Purge(int appId, ILog log)
        {
            var wrapLog = log?.Call($"{appId}");
            Purge(SystemRuntime.ZoneIdOfApp(appId), appId, log: log);
            wrapLog?.Invoke("ok");
        }

        /// <summary>
        /// Run some code and then purge the cache after that for full rebuild
        /// </summary>
        public static void DoAndPurge(int zoneId, int appId, Action action, bool global = false, ILog log = null)
        {
            var wrapLog = log?.Call($"{zoneId}, {appId}, fn(...), {global}");
            action.Invoke();
            Purge(zoneId, appId, global, log);
            wrapLog?.Invoke("ok");
        }
        #endregion

    }
}
