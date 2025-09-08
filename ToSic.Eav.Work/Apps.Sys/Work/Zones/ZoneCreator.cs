using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Repositories.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// Special tool just to create an app.
/// It's not part of the normal AppManager / ZoneManager, because when it's initialized it doesn't yet have a real app identity
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ZoneCreator(Generator<DbStorage, StorageOptions> db, AppCachePurger appCachePurger)
    : ServiceBase("Eav.AppBld", connect: [db, appCachePurger])
{

    public int Create(int tenantId, int siteId, string name, string appBasePath, string appBaseSharedPath)
    {
        var l = Log.Fn<int>($"create zone:{name}");
        var zoneId = db.New(new(null, null))
            .Zone.AddZone(tenantId, siteId, name, appBasePath, appBaseSharedPath);
        appCachePurger.PurgeZoneList();
        return l.Return(zoneId, $"created zone {zoneId}");
    }

}