using System.Linq;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps
{
    public sealed class AppFinder: HasLog
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

            var appId = _appStates.Apps(zoneId)
                .Where(p => p.Value == appName)
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
        private int AppIdFromFolderName(int zoneId, string folderName)
        {
            var wrapLog = Log.Fn<int>(folderName);
            var nameLower = folderName.ToLowerInvariant();

            var idOfAppWithMatchingName = -1;
            foreach (var p in _appStates.Apps(zoneId))
            {
                var appState = _appStates.Get(new AppIdentity(zoneId, p.Key));
                if (!string.IsNullOrEmpty(appState.Folder) && appState.Folder.ToLowerInvariant() == nameLower)
                    return wrapLog.Return(p.Key, "folder matched");
                if (!string.IsNullOrEmpty(appState.Name) && appState.Name.ToLowerInvariant() == nameLower)
                    idOfAppWithMatchingName = p.Key;
            }
            
            // Folder not found - let's check if there was an app with this name
            if (idOfAppWithMatchingName != -1)
                return wrapLog.Return(idOfAppWithMatchingName, "name matched");

            // not found
            return wrapLog.Return(AppConstants.AppIdNotFound, "not found");
        }

    }
}
