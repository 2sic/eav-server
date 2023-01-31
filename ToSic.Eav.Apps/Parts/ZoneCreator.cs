using ToSic.Lib.Logging;
using ToSic.Eav.Repository.Efc;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Special tool just to create an app.
    /// It's not part of the normal AppManager / ZoneManager, because when it's initialized it doesn't yet have a real app identity
    /// </summary>
    public class ZoneCreator: ServiceBase
    {
        #region Constructor / DI

        public ZoneCreator(DbDataController db, SystemManager systemManager) : base("Eav.AppBld") =>
            ConnectServices(
                _db = db,
                SystemManager = systemManager
            );
        private readonly DbDataController _db;
        protected readonly SystemManager SystemManager;


        #endregion

        public int Create(string name) => Log.Func($"create zone:{name}", () =>
        {
            var zoneId = _db.Init(null, null).Zone.AddZone(name);
            SystemManager.PurgeZoneList();
            return (zoneId, $"created zone {zoneId}");
        });

    }
}
