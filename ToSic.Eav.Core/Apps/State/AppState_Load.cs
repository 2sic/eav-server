using System;
using System.Linq;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
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
        public void Load(Action loader) => Log.Do(message: $"zone/app:{ZoneId}/{AppId} - Hash: {GetHashCode()}", timer: true, action: l =>
        {
            try
            {
                // first set a lock, to ensure that only one update/load is running at the same time
                lock (this)
                    l.Do($"loading: {Loading} (app loading start in lock)", action: () =>
                    {
                        // only if loading is true will the AppState object accept changes
                        Loading = true;
                        loader.Invoke();
                        CacheResetTimestamp("load complete");
                        EnsureNameAndFolderInitialized();
                        if (!FirstLoadCompleted) FirstLoadCompleted = true;

                        return $"done - dynamic load count: {DynamicUpdatesCount}";
                    });
            }
            catch (Exception ex)
            {
                l.A("Error");
                l.Ex(ex);
            }
            finally
            {
                // set loading to false again, to ensure that AppState won't accept changes
                Loading = false;
            }
        });

        private bool EnsureNameAndFolderInitialized() => Log.Func(l =>
        {
            // Before we do anything, check primary App
            // Otherwise other checks (like is name empty) will fail, because it's not empty
            // This is necessary, because it does get loaded with real settings
            // But we must override them to always be the same.
            if (NameId == PrimaryAppGuid)
            {
                Folder = PrimaryAppFolder;
                Name = PrimaryAppName;
                return (true, $"Primary App. Name: {Name}, Folder:{Folder}");
            }

            // Only do something if Name or Folder are still invalid
            if (!string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Folder))
                return (false, $"No change. Name: {Name}, Folder:{Folder}");

            // If the loader wasn't able to fill name/folder, then the data was not a json
            // so we must try to fix this now
            l.A("Trying to load Name/Folder from App package entity");
            var config = List.FirstOrDefault(md => md.Type.NameId == AppLoadConstants.TypeAppConfig);
            if (string.IsNullOrWhiteSpace(Name)) Name = config?.Value<string>(AppLoadConstants.FieldName);
            if (string.IsNullOrWhiteSpace(Folder)) Folder = config?.Value<string>(AppLoadConstants.FieldFolder);

            // Last corrections for the DefaultApp "Content"
            if (NameId == DefaultAppGuid)
            {
                // Always set the Name if we are on the content or primary app
                Name = ContentAppName;
                // Only set the folder if not over-configured since it can change in v13+
                if (string.IsNullOrWhiteSpace(Folder)) Folder = ContentAppFolder;
            }

            return (true, $"Name: {Name}, Folder:{Folder}");
        });
    }
}
