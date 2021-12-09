using System;
using System.Linq;
using ToSic.Eav.Documentation;

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


        [PrivateApi("should be internal, but ATM also used in FileAppStateLoader")]
        public void Load(Action loader)
        {
            var wrapLog = Log.Call(message: $"zone/app:{ZoneId}/{AppId} - Hash: {GetHashCode()}", useTimer: true);

            try
            {
                // first set a lock, to ensure that only one update/load is running at the same time
                lock (this)
                {
                    var inLockLog = Log.Call($"loading: {Loading}", "app loading start in lock");

                    // only if loading is true will the AppState object accept changes
                    Loading = true;
                    loader.Invoke();
                    CacheResetTimestamp();
                    EnsureNameAndFolderInitialized();
                    if (!FirstLoadCompleted) FirstLoadCompleted = true;

                    inLockLog($"done - dynamic load count: {DynamicUpdatesCount}");
                }
            }
            catch (Exception ex)
            {
                Log.Add("Error");
                Log.Exception(ex);
            }
            finally
            {
                // set loading to false again, to ensure that AppState won't accept changes
                Loading = false;

                wrapLog?.Invoke("ok");
            }
        }

        private void EnsureNameAndFolderInitialized()
        {
            var callLog = Log.Call($"Name: {Name}, Folder: {Folder}, AppGuidName: {AppGuidName}");
            // If the loader wasn't able to fill name/folder, then the data was not a json
            // so we must try to fix this now
            if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Folder))
            {
                // check if it's the default app
                if (AppGuidName == Constants.DefaultAppName)
                    Name = Folder = Constants.ContentAppName;
                else
                {
                    Log.Add("Trying to load Name/Folder from App package entity");
                    var config = List.FirstOrDefault(md => md.Type.StaticName == AppLoadConstants.TypeAppConfig);
                    Name = config?.Value<string>(AppLoadConstants.FieldName);
                    Folder = config?.Value<string>(AppLoadConstants.FieldFolder);
                }
            } 
            
            // if the folder still isn't know (either no data found, or the Name existed)
            // try one last time
            if (string.IsNullOrEmpty(Folder) && AppGuidName == Constants.DefaultAppName)
                Folder = Constants.ContentAppName;

            callLog($"Name: {Name}, Folder:{Folder}");
        }
    }
}
