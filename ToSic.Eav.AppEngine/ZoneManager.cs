using System;
using ToSic.Eav.Apps.Interfaces;
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
            //LanguageCache.Remove(ZoneId);
            SystemManager.PurgeZoneList();
        }


        //private static readonly Dictionary<int, List<DimensionDefinition>> LanguageCache =
        //    new Dictionary<int, List<DimensionDefinition>>();

        // todo: move retrieval to an interface, then move this read to ZoneRuntime!!!
        // 2017-06-25 old
        //public List<DimensionDefinition> Languages
        //{
        //    get
        //    {
        //        // note: cache at this level (to not instantiate a DB controller !) 
        //        if (!LanguageCache.ContainsKey(ZoneId))
        //        {
        //            LanguageCache[ZoneId] = DataController.Dimensions.GetLanguages(true).Cast<DimensionDefinition>()
        //                // ReSharper disable once SuspiciousTypeConversion.Global
        //                //.Select(d => new ZoneLanguagesTemp {Active = d.Active, EvironmentKey = d.EnvironmentKey})
        //                .ToList();
        //        }
        //        return LanguageCache[ZoneId];
        //    }
        //}


    #endregion

        
    }
    
}
