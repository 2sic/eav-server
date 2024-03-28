using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps.Internal.Work;

/// <summary>
/// Special tool just to create an app.
/// It's not part of the normal AppManager / ZoneManager, because when it's initialized it doesn't yet have a real app identity
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ZoneCreator(DbDataController db, AppCachePurger appCachePurger)
    : ServiceBase("Eav.AppBld", connect: [db, appCachePurger])
{

    public int Create(string name) 
    {
        var l = Log.Fn<int>($"create zone:{name}");
        var zoneId = db.Init(null, null).Zone.AddZone(name);
        appCachePurger.PurgeZoneList();
        return l.Return(zoneId, $"created zone {zoneId}");
    }

}