using System;
using System.Linq;
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
            var wrapLog = parentLog?.Call(message: $"zone/app:{ZoneId}/{AppId}", useTimer: true);

            try
            {
                // first set a lock, to ensure that only one update/load is running at the same time
                lock (this)
                {
                    // temporarily link logs, to put messages in both logs
                    Log.LinkTo(parentLog);
                    var inLockLog = Log.Call($"loading: {Loading}", "app loading start in lock");

                    // only if loading is true will the AppState object accept changes
                    Loading = true;
                    loader.Invoke();
                    CacheResetTimestamp();
                    EnsureNameAndFolderInitialized();
                    if(!FirstLoadCompleted) FirstLoadCompleted = true;

                    inLockLog($"done - dynamic load count: {DynamicUpdatesCount}");
                }
            }
            finally
            {
                // set loading to false again, to ensure that AppState won't accept changes
                Loading = false;

                // detach logs again, to prevent memory leaks because the global/cached app-state shouldn't hold on to temporary log objects
                wrapLog?.Invoke("ok");
                Log.Unlink();
            }
        }

        private void EnsureNameAndFolderInitialized()
        {
            // If the loader wasn't able to fill name/folder, then the data was not a json
            // so we must try to fix this now
            if (Name == null && Folder == null)
            {
                //var guidName = State.Cache.Zones[ZoneId].Apps[AppId];
                // check if it's the default app
                if (AppGuidName == Constants.DefaultAppName)
                    Name = Folder = Constants.ContentAppName;
                else
                {
                    var config = List.FirstOrDefault(md => md.Type.StaticName == AppLoadConstants.TypeAppConfig);
                    Name = config?.GetBestValue<string>(AppLoadConstants.FieldName);
                    Folder = config?.GetBestValue<string>(AppLoadConstants.FieldFolder);
                }
            }
        }
    }
}
