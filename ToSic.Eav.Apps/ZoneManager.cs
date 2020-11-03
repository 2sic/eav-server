using System;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps
{
    public class ZoneManager : ZoneBase<ZoneManager>
    {
        private readonly Lazy<DbDataController> _dbLazy;

        #region Constructor and simple properties

        public ZoneManager(Lazy<DbDataController> dbLazy) : base("App.Zone")
        {
            _dbLazy = dbLazy;
        }


        internal DbDataController DataController => _eavContext ?? (_eavContext = _dbLazy.Value.Init(ZoneId, null, Log));
        private DbDataController _eavContext;

        #endregion

        #region App management

        public void DeleteApp(int appId)
            => SystemManager.DoAndPurge(ZoneId, appId, () => DataController.App.DeleteApp(appId), true, Log);

        public int CreateApp()
        {
            Log.Add("create new app");
            var appGuid = Guid.NewGuid().ToString();
            var app = DataController.App.AddApp(null, appGuid);

            SystemManager.PurgeZoneList(Log);
            Log.Add($"app created a:{app.AppId}, guid:{appGuid}");
            return app.AppId;
        }

        #endregion

        #region Zone Management

        public int CreateZone(string name)
        {
            var wrapCall = Log.Call<int>($"create zone:{name}");
            var zoneId = _dbLazy.Value.Init(null, null, Log).Zone.AddZone(name);
            SystemManager.PurgeZoneList(Log);
            return wrapCall($"created zone {zoneId}", zoneId);
        }

        #endregion

        #region Language management


        public void SaveLanguage(string cultureCode, string cultureText, bool active)
        {
            Log.Add($"save languages code:{cultureCode}, txt:{cultureText}, act:{active}");
            DataController.Dimensions.AddOrUpdateLanguage(cultureCode, cultureText, active);
            SystemManager.PurgeZoneList(Log);
        }

        #endregion


    }

}
