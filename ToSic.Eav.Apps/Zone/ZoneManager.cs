using ToSic.Eav.Apps.Parts;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using ToSic.Eav.Repository.Efc;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps
{
    public class ZoneManager : ServiceBase, IZoneIdentity
    {
        #region Constructor and simple properties

        public ZoneManager(LazySvc<DbDataController> dbLazy, LazySvc<AppCachePurger> appCachePurger) : base("App.Zone")
        {
            ConnectServices(
                _dbLazy = dbLazy,
                _appCachePurger = appCachePurger
            );
        }
        private readonly LazySvc<DbDataController> _dbLazy;
        private readonly LazySvc<AppCachePurger> _appCachePurger;

        public int ZoneId { get; private set; }

        public ZoneManager SetId(int zoneId) 
        {
            ZoneId = zoneId;
            return this;
        }

        internal DbDataController DataController => _db ?? (_db = _dbLazy.Value.Init(ZoneId, null));
        private DbDataController _db;

        #endregion

        #region App management

        public void DeleteApp(int appId, bool fullDelete)
            => _appCachePurger.Value.DoAndPurge(ZoneId, appId, () => DataController.App.DeleteApp(appId, fullDelete), true);


        #endregion

        #region Language management

        public void SaveLanguage(string cultureCode, string cultureText, bool active)
        {
            var l = Log.Fn($"save languages code:{cultureCode}, txt:{cultureText}, act:{active}");
            DataController.Dimensions.AddOrUpdateLanguage(cultureCode, cultureText, active);
            _appCachePurger.Value.PurgeZoneList();
            l.Done();
        }

        #endregion


    }

}
