using System.Linq;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps
{
    public sealed class AppFinder: ServiceBase
    {
        #region Constructor / DI

        public AppFinder(IAppStates appStates) : base("App.ZoneRt") => _appStates = appStates;

        private readonly IAppStates _appStates;

        #endregion

        /// <summary>
        /// Find the app id from the app-name (usually a guid or "Default").
        /// Can also check the folder name
        /// </summary>
        public int FindAppId(int zoneId, string appName, bool alsoCheckFolderName = false)
        {
            var wrapLog =
                Log.Fn<int>(
                    $"{nameof(zoneId)}:{zoneId}, {nameof(appName)}:{appName}, {nameof(alsoCheckFolderName)}:{alsoCheckFolderName}");

            if (string.IsNullOrEmpty(appName))
                return wrapLog.Return(Constants.AppIdEmpty, "no name");

            var nameLower = appName.ToLowerInvariant();
            var appId = _appStates.Apps(zoneId)
                .Where(p => p.Value.ToLowerInvariant() == nameLower)
                .Select(p => p.Key).FirstOrDefault();

            // optionally check folder names
            if (appId == 0 && alsoCheckFolderName)
                appId = AppIdFromFolderName(zoneId, appName);

            var final = appId > 0 ? appId : AppConstants.AppIdNotFound;
            return wrapLog.ReturnAndLog(final);
        }

        /// <summary>
        /// Find an app based on the folder name. Will check the App Metadata for this
        /// </summary>
        public int AppIdFromFolderName(int zoneId, string folderName)
        {
            var wrapLog = Log.Fn<int>(folderName);
            var nameLower = folderName.ToLowerInvariant();

            foreach (var p in _appStates.Apps(zoneId))
            {
                var appState = _appStates.Get(new AppIdentity(zoneId, p.Key));
                if (!string.IsNullOrEmpty(appState.Folder) && appState.Folder.ToLowerInvariant() == nameLower)
                    return wrapLog.Return(p.Key, "folder matched");
            }

            // not found
            return wrapLog.Return(AppConstants.AppIdNotFound, "not found");
        }

        /// <summary>
        /// Find an app based on the app name. Will check the App Metadata for this
        /// </summary>
        public int AppIdFromAppName(int zoneId, string appName)
        {
            var wrapLog = Log.Fn<int>(appName);
            var nameLower = appName.ToLowerInvariant();

            foreach (var p in _appStates.Apps(zoneId))
            {
                var appState = _appStates.Get(new AppIdentity(zoneId, p.Key));

                if (!string.IsNullOrEmpty(appState.Name) && appState.Name.ToLowerInvariant() == nameLower)
                    return wrapLog.Return(p.Key, "name matched");
            }

            // not found
            return wrapLog.Return(AppConstants.AppIdNotFound, "not found");
        }
    }
}
