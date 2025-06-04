using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps.Internal.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ZoneManager(LazySvc<DbDataController> dbLazy, LazySvc<AppCachePurger> appCachePurger)
    : ServiceBase("App.Zone", connect: [dbLazy, appCachePurger]), IZoneIdentity
{

    public int ZoneId { get; private set; }

    public ZoneManager SetId(int zoneId) 
    {
        ZoneId = zoneId;
        return this;
    }

    internal DbDataController DataController => field ??= dbLazy.Value.Init(ZoneId, null);


    #region App management

    public void DeleteApp(int appId, bool fullDelete)
        => appCachePurger.Value.DoAndPurge(ZoneId, appId, () => DataController.App.DeleteApp(appId, fullDelete), true);


    #endregion

    #region Language management

    public void SaveLanguage(string cultureCode, string cultureText, bool active)
    {
        var l = Log.Fn($"save languages code:{cultureCode}, txt:{cultureText}, act:{active}");
        DataController.Dimensions.AddOrUpdateLanguage(cultureCode, cultureText, active);
        appCachePurger.Value.PurgeZoneList();
        l.Done();
    }

    #endregion


}