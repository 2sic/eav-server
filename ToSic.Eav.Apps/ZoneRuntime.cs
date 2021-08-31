﻿using ToSic.Eav.Apps.Parts;

namespace ToSic.Eav.Apps
{
    public class ZoneRuntime: ZoneBase<ZoneRuntime>
    {

        #region Constructor / DI

        public ZoneRuntime() : base("App.ZoneRt") { }

        protected ZoneRuntime(string logName = null): base(logName)
        {
            //_appStates = appStates;
        }
        //private readonly IAppStates _appStates;

        #endregion

        // todo: put this in the IAppStates
        //public int DefaultAppId => Cache.Zones[ZoneId].DefaultAppId;

        //public Dictionary<int, string> Apps => Cache.Zones[ZoneId].Apps;

        //public List<DimensionDefinition> Languages(bool includeInactive = false) => includeInactive 
        //    ? Cache.Zones[ZoneId].Languages 
        //    : Cache.Zones[ZoneId].Languages.Where(l => l.Active).ToList();

        ///// <summary>
        ///// Find the app id from the app-name (usually a guid or "Default").
        ///// Can also check the folder name
        ///// </summary>
        ///// <returns></returns>
        //public int FindAppId(int zoneId, string appName, bool alsoCheckFolderName = false)
        //{
        //    //var zones = State.Zones;

        //    if (string.IsNullOrEmpty(appName))
        //        return Constants.AppIdEmpty;

        //    var appId = _appStates.Apps(zoneId) // zones[ZoneId].Apps
        //        .Where(p => p.Value == appName)
        //        .Select(p => p.Key).FirstOrDefault();

        //    // optionally check folder names
        //    if (appId == 0 && alsoCheckFolderName)
        //        appId = AppIdFromFolderName(appName);

        //    return appId > 0 ? appId : AppConstants.AppIdNotFound;
        //}

        ///// <summary>
        ///// Find an app based on the folder name. Will check the App Metadata for this
        ///// </summary>
        ///// <param name="folderName"></param>
        ///// <returns></returns>
        //private int AppIdFromFolderName(string folderName)
        //{
        //    var wrapLog = Log.Call<int>(folderName);
        //    var nameLower = folderName.ToLowerInvariant();

        //    var idOfAppWithMatchingName = -1;
        //    foreach (var p in _appStates.Apps(ZoneId) /*State.Zones[ZoneId].Apps*/)
        //    {
        //        var appState = _appStates.Get(new AppIdentity(ZoneId, p.Key));
        //        if (!string.IsNullOrEmpty(appState.Folder) && appState.Folder.ToLowerInvariant() == nameLower)
        //            return wrapLog("folder matched", p.Key);
        //        if (!string.IsNullOrEmpty(appState.Name) && appState.Name.ToLowerInvariant() == nameLower)
        //            idOfAppWithMatchingName = p.Key;
        //    }
            
        //    // Folder not found - let's check if there was an app with this name
        //    if (idOfAppWithMatchingName != -1)
        //        return wrapLog("name matched", idOfAppWithMatchingName);

        //    // not found
        //    return wrapLog("not found", AppConstants.AppIdNotFound);
        //}

    }
}
