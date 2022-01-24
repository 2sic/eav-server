using System;
using System.Linq;
using ToSic.Eav.Documentation;
using static ToSic.Eav.Constants;

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
                    CacheResetTimestamp("load complete");
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

        private bool EnsureNameAndFolderInitialized()
        {
            var callLog = Log.Call<bool>($"Name: {Name}, Folder: {Folder}, AppGuidName: {NameId}");

            // Only do something if Name or Folder are still invalid
            if (!string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Folder))
                return callLog($"No change. Name: {Name}, Folder:{Folder}", false);

            // If the loader wasn't able to fill name/folder, then the data was not a json
            // so we must try to fix this now
            var config = List.FirstOrDefault(md => md.Type.NameId == AppLoadConstants.TypeAppConfig);

            Log.Add("Trying to load Name/Folder from App package entity");
            if (string.IsNullOrWhiteSpace(Name)) Name = config?.Value<string>(AppLoadConstants.FieldName);
            if (string.IsNullOrWhiteSpace(Folder)) Folder = config?.Value<string>(AppLoadConstants.FieldFolder);

            // if the folder still isn't know (either no data found, or the Name existed)
            // try one last time
            if (string.IsNullOrWhiteSpace(Folder))
            {
                if (NameId == DefaultAppGuid) Folder = ContentAppFolder;
                else if (NameId == PrimaryAppGuid) Folder = PrimaryAppFolder; // #SiteApp v13
            }
            if (string.IsNullOrWhiteSpace(Name))
            {
                if (NameId == DefaultAppGuid) Name = ContentAppName;
                else if (NameId == PrimaryAppGuid) Name = PrimaryAppName; // #SiteApp v13
            }

            return callLog($"Name: {Name}, Folder:{Folder}", true);
        }
    }
}
