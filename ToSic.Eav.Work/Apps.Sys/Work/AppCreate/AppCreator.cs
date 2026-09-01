using System.Text.RegularExpressions;
using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Repositories.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// Special tool just to create an app.
/// It's not part of the normal AppManager / ZoneManager, because when it's initialized it doesn't yet have a real app identity
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppCreator(
    Generator<DbStorage, StorageOptions> db,
    IAppsAndZonesLoaderWithRaw appsAndZonesLoader,
    AppCachePurger appCachePurger,
    AppWorkQuick<AppInitializer> appInitGenerator)
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
    public void Create(string appName, string? appGuid = null, int? inheritAppId = null)
    {
        var isDefaultAppName = appName is KnownAppsConstants.ContentAppName or KnownAppsConstants.DefaultAppGuid;
        if (string.IsNullOrEmpty(appName) 
            || !Regex.IsMatch(appName, "^[0-9A-Za-z -_]+$") 
            || isDefaultAppName && !inheritAppId.HasValue)
            throw new ArgumentOutOfRangeException("appName '" + appName + "' not allowed");

        var appId = isDefaultAppName
            ? ConfigureExistingDefaultApp(inheritAppId!.Value)
            : CreateInDb(appGuid ?? Guid.NewGuid().ToString(), inheritAppId);

        // must get app from DB directly, not from cache, so no State.Get(...)
        var appReader = appsAndZonesLoader.AppReaderRaw(appId, new());

        appInitGenerator.New(appReader).InitializeApp(/*appReader,*/ appName, new());
    }

    private int ConfigureExistingDefaultApp(int inheritAppId)
    {
        var l = Log.Fn<int>($"inherit:{inheritAppId}");
        var dbStorage = db.New(new(_zoneId, null, inheritAppId));
        var appId = dbStorage.SqlDb.TsDynDataApps
            .Where(a => a.ZoneId == _zoneId && a.Name == KnownAppsConstants.DefaultAppGuid)
            .Select(a => a.AppId)
            .Single();

        dbStorage.App.SetInheritanceAndSave(appId, inheritAppId);
        appCachePurger.PurgeZoneList();
        l.A($"default app configured a:{appId}, inherit:{inheritAppId}");
        return l.Return(appId);
    }

    private int CreateInDb(string appGuid, int? inheritAppId)
    {
        var l = Log.Fn<int>("create new app");

        var app = db.New(new(_zoneId, null, inheritAppId))
            .App.AddAppAndSave(_zoneId, appGuid, inheritAppId);

        appCachePurger.PurgeZoneList();
        l.A($"app created a:{app.AppId}, guid:{appGuid}");
        return l.Return(app.AppId);
    }

}