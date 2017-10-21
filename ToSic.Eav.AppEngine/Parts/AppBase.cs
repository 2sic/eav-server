using ToSic.Eav.App;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps.Parts
{
    public class AppBase: HasLog, IApp
    {
        #region Constructor and simple properties
        #region basic properties
        /// <inheritdoc />
        /// <summary>
        /// The zone id of this app
        /// </summary>
        public int ZoneId { get; }

        /// <inheritdoc />
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
        /// <param name="parentLog"></param>
        public AppBase(int zoneId, int appId, Log parentLog) : base("App.Base", parentLog)
        {
            ZoneId = zoneId;
            AppId = appId;
        }

        public AppBase(IApp app, Log parentLog) : this(app.ZoneId, app.AppId, parentLog) { }

        public AppBase(int appId, Log parentLog) : this(((BaseCache) DataSource.GetCache(null)).GetZoneAppId(appId: appId).Item1, appId, parentLog) { }

        internal AppBase(IDataSource data, Log parentLog) : this(data.ZoneId, data.AppId, parentLog)
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
        internal AppDataPackage Package => Cache.AppDataPackage;


        #endregion

        #endregion


    }
}
