using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps.Parts
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

        #region logging

        protected Log Log = new Log("AppBas");
        #endregion

        /// <summary>
        /// Create an app manager for this specific app
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        /// <param name="parentLog"></param>
        public AppBase(int zoneId, int appId, Log parentLog = null)
        {
            ZoneId = zoneId;
            AppId = appId;
            Log.LinkTo(parentLog);
        }

        public AppBase(IApp app, Log parentLog = null) : this(app.ZoneId, app.AppId, parentLog) { }

        public AppBase(int appId, Log parentLog = null) : this(((BaseCache) DataSource.GetCache(null)).GetZoneAppId(appId: appId).Item1, appId, parentLog) { }

        internal AppBase(IDataSource data, Log parentLog = null) : this(data.ZoneId, data.AppId, parentLog)
        {
            _data = data;
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
