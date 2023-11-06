using ToSic.Eav.Repository.Efc;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// Special tool just to create an app.
    /// It's not part of the normal AppManager / ZoneManager, because when it's initialized it doesn't yet have a real app identity
    /// </summary>
    public class ZoneCreator: ServiceBase
    {
        #region Constructor / DI

        public ZoneCreator(DbDataController db, AppCachePurger appCachePurger) : base("Eav.AppBld") =>
            ConnectServices(
                _db = db,
                AppCachePurger = appCachePurger
            );
        private readonly DbDataController _db;
        protected readonly AppCachePurger AppCachePurger;


        #endregion

        public int Create(string name) 
        {
            var l = Log.Fn<int>($"create zone:{name}");
            var zoneId = _db.Init(null, null).Zone.AddZone(name);
            AppCachePurger.PurgeZoneList();
            return l.Return(zoneId, $"created zone {zoneId}");
        }

    }
}
