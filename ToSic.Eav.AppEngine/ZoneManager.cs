using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.Persistence;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps
{
    public class ZoneManager : ZoneBase
    {
        #region Constructor and simple properties

        public ZoneManager(int zoneId) : base(zoneId)
        {
        }

        internal DbDataController DataController => _eavContext ?? (_eavContext = DbDataController.Instance(ZoneId));
        private DbDataController _eavContext;

        #endregion

        #region App management

        public void DeleteApp(int appId)
            => SystemManager.DoAndPurge(ZoneId, appId, () => DataController.App.DeleteApp(appId), true);

        public int CreateApp()
        {
            var app = DataController.App.AddApp(null, Guid.NewGuid().ToString());

            SystemManager.PurgeZoneList();
            return app.AppId;
        }

        #endregion

        #region Zone Management

        public static int CreateZone(string name)
        {
            var zoneId = DbDataController.Instance().Zone.AddZone(name);
            SystemManager.PurgeZoneList();
            return zoneId;
        }

        #endregion

        #region Language management


        public void SaveLanguage(string cultureCode, string cultureText, bool active)
        {
            DataController.Dimensions.AddOrUpdateLanguage(cultureCode, cultureText, active);
            SystemManager.PurgeZoneList();
        }

        #endregion


    }

}
