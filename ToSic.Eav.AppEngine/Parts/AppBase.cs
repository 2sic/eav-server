using ToSic.Eav.App;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Logging.Simple;

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
        protected AppBase(int zoneId, int appId, Log parentLog) : base(zoneId, appId, parentLog, "App.Base")
        {
        }

        protected AppBase(IAppIdentity app, Log parentLog) : this(app.ZoneId, app.AppId, parentLog) { }

        protected AppBase(int appId, Log parentLog) : this(((BaseCache) DataSource.GetCache(null)).GetZoneAppId(appId: appId).Item1, appId, parentLog) { }

        protected AppBase(IDataSource data, Log parentLog) : this(data.ZoneId, data.AppId, parentLog)
        {
            _data = data;
        }


        #region Data & Cache
        internal BaseCache Cache => _cache ?? (_cache = (BaseCache) Data.Cache);
        private BaseCache _cache;

        internal IDataSource Data => _data ?? (_data = DataSource.GetInitialDataSource(ZoneId, AppId));
        private IDataSource _data;

        /// <summary>
        /// The cache-package if needed (mainly for export/import, where the full data is necessary)
        /// </summary>
        public AppDataPackage Package => Cache.AppDataPackage;


        #endregion


    }
}
