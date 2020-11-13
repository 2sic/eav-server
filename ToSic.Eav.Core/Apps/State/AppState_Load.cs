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
            var wrapLog = Log.Call(message: $"zone/app:{ZoneId}/{AppId}", useTimer: true);
            Loading = true;
            // temporarily link logs, to put messages in both logs
            Log.LinkTo(parentLog);
            Log.Add("app loading start");
            loader.Invoke();
            CacheResetTimestamp();

            // If the loader wasn't able to fill name/folder, then the data was not a json
            // so we must try to fix this now
            if (Name == null && Folder == null)
            {
                var guidName = State.Cache.Zones[ZoneId].Apps[AppId];
                // check if it's the default app
                if (guidName == Constants.DefaultAppName)
                    Name = Folder = Constants.ContentAppName;
                else
                {
                    var config = List.FirstOrDefault(md => md.Type.StaticName == AppLoadConstants.TypeAppConfig);
                    Name = config?.GetBestValue<string>(AppLoadConstants.FieldName);
                    Folder = config?.GetBestValue<string>(AppLoadConstants.FieldFolder);
                }
            }

            Loading = false;
            FirstLoadCompleted = true;
            Log.Add($"app loading done - dynamic load count: {DynamicUpdatesCount}");
            // detach logs again, to prevent memory leaks
            Log.LinkTo(null);
            wrapLog("ok");
        }

    }
}
