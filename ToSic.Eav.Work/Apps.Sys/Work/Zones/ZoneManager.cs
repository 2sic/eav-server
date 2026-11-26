using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Repositories.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ZoneManager(Generator<DbStorage, StorageOptions> dbLazy, LazySvc<IAppCachePurge> appCachePurge, LazySvc<IZoneCachePurge> zoneCachePurge)
    : ServiceBase("App.Zone", connect: [dbLazy, appCachePurge, zoneCachePurge]), IZoneIdentity
{

    public int ZoneId { get; private set; }

    public ZoneManager SetId(int zoneId) 
    {
        ZoneId = zoneId;
        return this;
    }

    [field: AllowNull, MaybeNull]
    internal DbStorage DbStorage => field ??= dbLazy.New(new(ZoneId, null));


    #region App management

    public void DeleteApp(int appId, bool fullDelete)
    {
        var appIdentity = new AppIdentity(ZoneId, appId);
        var l = Log.Fn($"AppIdentity: {appIdentity.Show()}");
        DbStorage.App.DeleteApp(appId, fullDelete);
        // Purge the app itself, ensuring that pre-purge/cleanup happens.
        appCachePurge.Value.Purge(appIdentity);
        // TODO: IDEALLY this would optimize to only purge the correct zone,
        // but for now we just purge all zone
        zoneCachePurge.Value.PurgeZoneList();
        l.Done();
    }

    #endregion

    #region Language management

    public void SaveLanguage(string cultureCode, string cultureText, bool active)
    {
        var l = Log.Fn($"save languages code:{cultureCode}, txt:{cultureText}, act:{active}");
        DbStorage.Dimensions.AddOrUpdateZoneDimension(cultureCode, cultureText, active);
        zoneCachePurge.Value.PurgeZoneList();
        l.Done();
    }

    #endregion


}