using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav.Apps.Interfaces
{
    public class AppBase: IApp
    {
        #region Constructor and simple properties
        #region basic properties
        /// <summary>
        /// The zone id of this app
        /// </summary>
        public int ZoneId { get; }

        /// <summary>
        /// The app id
        /// </summary>
        public int AppId { get; }
        #endregion


        /// <summary>
        /// Create an app manager for this specific app
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        public AppBase(int zoneId, int appId)
        {
            ZoneId = zoneId;
            AppId = appId;
        }

        public AppBase(IApp app)
        {
            ZoneId = app.ZoneId;
            AppId = app.AppId;
        }

        public AppBase(int appId)
        {
            AppId = appId;
            ZoneId = ((BaseCache) DataSource.GetCache(null)).GetZoneAppId(appId: appId).Item1;
        }

        internal AppBase(IDataSource data)
        {
            _data = data;
            AppId = _data.AppId;
            ZoneId = _data.ZoneId;
        }


        #region Data & Cache
        internal BaseCache Cache => _cache ?? (_cache = (BaseCache) Data.Cache);
        private BaseCache _cache;

        internal IDataSource Data => _data ?? (_data = DataSource.GetInitialDataSource(ZoneId, AppId));
        private IDataSource _data;

        #endregion

        #endregion


    }
}
