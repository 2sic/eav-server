using ToSic.Eav.Logging;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Special tool just to create an app.
    /// It's not part of the normal AppManager / ZoneManager, because when it's initialized it doesn't yet have a real app identity
    /// </summary>
    public class ZoneCreator: HasLog<ZoneCreator>
    {
        #region Constructor / DI

        private readonly DbDataController _db;

        public ZoneCreator(DbDataController db) : base("Eav.AppBld") => _db = db;

        #endregion

        public int Create(string name)
        {
            var wrapCall = Log.Call<int>($"create zone:{name}");
            var zoneId = _db.Init(null, null, Log).Zone.AddZone(name);
            SystemManager.PurgeZoneList(Log);
            return wrapCall($"created zone {zoneId}", zoneId);
        }

    }
}
