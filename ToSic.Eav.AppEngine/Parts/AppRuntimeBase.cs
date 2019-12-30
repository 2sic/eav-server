using ToSic.Eav.DataSources;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Root class for app runtime objects
    /// </summary>
    public abstract class AppRuntimeBase: AppBase
    {
        private const string LogId = "App.Base";

        ///// <summary>
        ///// Create an app manager for this specific app
        ///// </summary>
        ///// <param name="zoneId"></param>
        ///// <param name="appId"></param>
        ///// <param name="parentLog"></param>
        //protected AppRuntimeBase(int zoneId, int appId, ILog parentLog) 
        //    : base(zoneId, appId, parentLog, LogId)
        //{
        //}

        protected AppRuntimeBase(IAppIdentity app, ILog parentLog) 
            : base(app, new CodeRef(),  parentLog, LogId) { }

        //protected AppRuntimeBase(int appId, ILog parentLog) : this(Factory.GetAppsCache().GetIdentity(appId: appId), parentLog) { }

        protected AppRuntimeBase(IDataSource data, ILog parentLog) : this(data as IAppIdentity, parentLog)
        {
            _data = data;
        }


        #region Data & Cache
        //public AppRoot Cache => _cache ?? (_cache = (AppRoot) Data/*.Root*/);
        //private AppRoot _cache;

        public IDataSource Data => _data ?? (_data = new DataSource(Log).GetPublishing(this));
        private IDataSource _data;

        /// <summary>
        /// The cache-package if needed (mainly for export/import, where the full data is necessary)
        /// </summary>
        public AppState AppState => /*Factory.GetAppState*/Eav.Apps.Apps.Get(this);// Cache.AppState;


        #endregion


    }
}
