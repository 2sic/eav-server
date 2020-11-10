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

        public void DeleteApp(int appId, bool fullDelete)
            => SystemManager.DoAndPurge(ZoneId, appId, () => DataController.App.DeleteApp(appId, fullDelete), true, Log);


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
