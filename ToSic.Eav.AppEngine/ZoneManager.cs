using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.BLL;

namespace ToSic.Eav.Apps
{
    public class ZoneManager: ZoneBase
    {
        #region Constructor and simple properties
        public ZoneManager(int zoneId) : base(zoneId)
        {
        }

        internal EavDataController DataController => _eavContext ?? (_eavContext = EavDataController.Instance(ZoneId));
        private EavDataController _eavContext;
        #endregion

        #region App management
        public void DeleteApp(int appId) => State.DoAndPurge(ZoneId, appId, () => DataController.App.DeleteApp(appId), true);

        public int CreateApp()
        {
            var app = DataController.App.AddApp(null, Guid.NewGuid().ToString());

            State.Purge(ZoneId, app.AppID, true);
            return app.AppID;
        }
        #endregion

        #region Zone Management

        public static int CreateZone(string name)
        {
            var zoneId = EavDataController.Instance().Zone.AddZone(name).Item1.ZoneID;
            State.Purge(0, 0, true);
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
