using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Root class for app runtime objects
    /// </summary>
    public abstract class AppBase: AppIdentity
    {
        /// <summary>
        /// Create an app manager for this specific app
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        /// <param name="parentLog"></param>
        protected AppBase(int zoneId, int appId, ILog parentLog) 
            : base(zoneId, appId, parentLog, "App.Base")
        {
        }

        protected AppBase(IInAppAndZone app, ILog parentLog) : this(app.ZoneId, app.AppId, parentLog) { }

        protected AppBase(int appId, ILog parentLog) : this(((RootCache) DataSource.GetCache(null)).GetZoneAppId(appId: appId).Item1, appId, parentLog) { }

        protected AppBase(IDataSource data, ILog parentLog) : this(data.ZoneId, data.AppId, parentLog)
        {
            _data = data;
        }


        #region Data & Cache
        public RootCache Cache => _cache ?? (_cache = (RootCache) Data.Cache);
        private RootCache _cache;

        public IDataSource Data => _data ?? (_data = DataSource.GetInitialDataSource(ZoneId, AppId));
        private IDataSource _data;

        /// <summary>
        /// The cache-package if needed (mainly for export/import, where the full data is necessary)
        /// </summary>
        public AppState Package => Cache.AppState;


        #endregion


    }
}
