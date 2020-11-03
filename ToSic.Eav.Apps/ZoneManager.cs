using System;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps
{
    public class ZoneManager : ZoneBase<ZoneManager>
    {
        #region Constructor and simple properties

        public ZoneManager() : base("App.Zone") {}


        internal DbDataController DataController => _eavContext ?? (_eavContext = DbDataController.Instance(ZoneId, null, Log));
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

        public static int CreateZone(string name, ILog parentLog)
        {
            var log = new Log("App.ZoneMg", parentLog, $"create zone:{name}");
            var zoneId = DbDataController.Instance(null, null, parentLog).Zone.AddZone(name);
            SystemManager.PurgeZoneList(parentLog);
            return zoneId;
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
