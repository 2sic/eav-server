using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Repository.EF4;

namespace ToSic.Eav.Apps
{
    public class ZoneManager: ZoneBase
    {
        #region Constructor and simple properties
        public ZoneManager(int zoneId) : base(zoneId)
        {
        }

        internal DbDataController DataController => _eavContext ?? (_eavContext = DbDataController.Instance(ZoneId));
        private DbDataController _eavContext;
        #endregion

        #region App management
        public void DeleteApp(int appId) => SystemManager.DoAndPurge(ZoneId, appId, () => DataController.App.DeleteApp(appId), true);

        public int CreateApp()
        {
            var app = DataController.App.AddApp(null, Guid.NewGuid().ToString());

            SystemManager.PurgeEverything();
            return app.AppID;
        }
        #endregion

        #region Zone Management

        public static int CreateZone(string name)
        {
            var zoneId = DbDataController.Instance().Zone.AddZone(name).Item1.ZoneID;
            SystemManager.PurgeEverything();
            return zoneId;
        }

        #endregion

        #region Language management


        public void SaveLanguage(string cultureCode, string cultureText, bool active)
            => DataController.Dimensions.AddOrUpdateLanguage(cultureCode, cultureText, active);



        // todo: get from cache (it seem to do this already!) & then move to ZoneRuntime!!!
        public List<ZoneLanguagesTemp> Languages => DataController.Dimensions.GetLanguages()
            // ReSharper disable once SuspiciousTypeConversion.Global
            .Select(d => new ZoneLanguagesTemp() { Active  = d.Active, TennantKey = d.ExternalKey})
            .ToList();
        #endregion

        
    }

    public class ZoneLanguagesTemp
    {
        public string TennantKey;
        public bool Active;
    }
}
