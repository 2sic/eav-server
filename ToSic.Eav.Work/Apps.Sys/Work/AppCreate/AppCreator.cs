using System.Text.RegularExpressions;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Repositories;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps.Internal.Work;

/// <summary>
/// Special tool just to create an app.
/// It's not part of the normal AppManager / ZoneManager, because when it's initialized it doesn't yet have a real app identity
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppCreator(
    DbDataController db,
    IAppsAndZonesLoaderWithRaw appsAndZonesLoader,
    AppCachePurger appCachePurger,
    Generator<AppInitializer> appInitGenerator)
    : ServiceBase("Eav.AppBld", connect: [db, appInitGenerator, appCachePurger, appsAndZonesLoader])
{
    #region Constructor / DI

    private int _zoneId;

    public AppCreator Init(int zoneId)
    {
        _zoneId = zoneId;
        return this;
    }

    #endregion

    /// <summary>
    /// Will create a new app in the system and initialize the basic settings incl. the 
    /// app-definition
    /// </summary>
    /// <returns></returns>
    public void Create(string appName, string appGuid = null, int? inheritAppId = null)
    {
        // check if invalid app-name which should never be created like this
        if (appName == KnownAppsConstants.ContentAppName || appName == KnownAppsConstants.DefaultAppGuid || string.IsNullOrEmpty(appName) || !Regex.IsMatch(appName, "^[0-9A-Za-z -_]+$"))
            throw new ArgumentOutOfRangeException("appName '" + appName + "' not allowed");

        var appId = CreateInDb(appGuid ?? Guid.NewGuid().ToString(), inheritAppId);

        // must get app from DB directly, not from cache, so no State.Get(...)
        var appState = appsAndZonesLoader.AppReaderRaw(appId, new());

        appInitGenerator.New().InitializeApp(appState, appName, new());
    }

    private int CreateInDb(string appGuid, int? inheritAppId)
    {
        var l = Log.Fn<int>("create new app");
        var app = db.Init(_zoneId, null, inheritAppId).App.AddApp(null, appGuid, inheritAppId);

        appCachePurger.PurgeZoneList();
        l.A($"app created a:{app.AppId}, guid:{appGuid}");
        return l.Return(app.AppId);
    }

}