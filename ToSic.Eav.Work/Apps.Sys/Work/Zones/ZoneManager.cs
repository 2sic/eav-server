using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Apps.Internal.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ZoneManager(LazySvc<DbStorage> dbLazy, LazySvc<AppCachePurger> appCachePurger)
    : ServiceBase("App.Zone", connect: [dbLazy, appCachePurger]), IZoneIdentity
{

    public int ZoneId { get; private set; }

    public ZoneManager SetId(int zoneId) 
    {
        ZoneId = zoneId;
        return this;
    }

    internal DbStorage DbStorage => field ??= dbLazy.Value.Init(ZoneId, null);


    #region App management

    public void DeleteApp(int appId, bool fullDelete)
        => appCachePurger.Value.DoAndPurge(ZoneId, appId, () => DbStorage.App.DeleteApp(appId, fullDelete), true);


    #endregion

    #region Language management

    public void SaveLanguage(string cultureCode, string cultureText, bool active)
    {
        var l = Log.Fn($"save languages code:{cultureCode}, txt:{cultureText}, act:{active}");
        DbStorage.Dimensions.AddOrUpdateLanguage(cultureCode, cultureText, active);
        appCachePurger.Value.PurgeZoneList();
        l.Done();
    }

    #endregion


}