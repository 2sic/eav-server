using System;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps
{
    public partial class AppState
    {
        /// <summary>
        /// Shows that the app is loading / building up the data.
        /// </summary>
        protected bool Loading;

        /// <summary>
        /// Shows that the initial load has completed
        /// </summary>
        protected bool FirstLoadCompleted;

        /// <summary>
        /// Show how many times the app has been Dynamically updated - in case we run into cache rebuild problems.
        /// </summary>
        public int DynamicUpdatesCount;



        internal void Load(ILog parentLog, Action loader)
        {
            var wrapLog = Log.Call(message: $"zone/app:{ZoneId}/{AppId}", useTimer: true);
            Loading = true;
            // temporarily link logs, to put messages in both logs
            Log.LinkTo(parentLog);
            Log.Add("app loading start");
            loader.Invoke();
            CacheResetTimestamp();
            Loading = false;
            FirstLoadCompleted = true;
            Log.Add($"app loading done - dynamic load count: {DynamicUpdatesCount}");
            // detach logs again, to prevent memory leaks
            Log.LinkTo(null);
            wrapLog("ok");
        }

    }
}
