using System;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps
{
    public class ZoneManager : ZoneBase<ZoneManager>
    {
        #region Constructor and simple properties

        public ZoneManager(Lazy<DbDataController> dbLazy, Lazy<SystemManager> systemManagerLazy) : base("App.Zone")
        {
            _dbLazy = dbLazy;
            _systemManagerLazy = systemManagerLazy;
        }
        private readonly Lazy<DbDataController> _dbLazy;
        private readonly Lazy<SystemManager> _systemManagerLazy;


        internal DbDataController DataController => _eavContext ?? (_eavContext = _dbLazy.Value.Init(ZoneId, null, Log));
        private DbDataController _eavContext;

        #endregion

        #region App management

        public void DeleteApp(int appId, bool fullDelete)
            => _systemManagerLazy.Value.Init(Log).DoAndPurge(ZoneId, appId, () => DataController.App.DeleteApp(appId, fullDelete), true);


        #endregion

        #region Language management

        public void SaveLanguage(string cultureCode, string cultureText, bool active)
        {
            Log.Add($"save languages code:{cultureCode}, txt:{cultureText}, act:{active}");
            DataController.Dimensions.AddOrUpdateLanguage(cultureCode, cultureText, active);
            _systemManagerLazy.Value.Init(Log).PurgeZoneList();
        }

        #endregion


    }

}
