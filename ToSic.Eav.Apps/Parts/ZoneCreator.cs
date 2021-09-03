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

        public ZoneCreator(DbDataController db, SystemManager systemManager) : base("Eav.AppBld")
        {
            _db = db;
            SystemManager = systemManager.Init(Log);
        }
        private readonly DbDataController _db;
        protected readonly SystemManager SystemManager;


        #endregion

        public int Create(string name)
        {
            var wrapCall = Log.Call<int>($"create zone:{name}");
            var zoneId = _db.Init(null, null, Log).Zone.AddZone(name);
            SystemManager.PurgeZoneList();
            return wrapCall($"created zone {zoneId}", zoneId);
        }

    }
}
