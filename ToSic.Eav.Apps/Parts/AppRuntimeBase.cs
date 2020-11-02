using ToSic.Eav.DataSources;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Root class for app runtime objects
    /// </summary>
    public abstract class AppRuntimeBase<T>: AppBase where T: AppRuntimeBase<T>
    {
        public bool ShowDrafts { get; private set; }

        #region Constructor / DI

        protected AppRuntimeBase(string logName): base(logName, new CodeRef()) { }

        public T Init(IAppIdentity app, bool showDrafts, ILog parentLog)
        {
            Init(app, new CodeRef(), parentLog);
            // re-use data of parent if it's constructed from an app-manager
            if (app is AppManager parentIsAppManager)
                _data = parentIsAppManager.Data;
            //if (app is IDataSource dataSource)
            //    _data = dataSource;
            ShowDrafts = showDrafts;
            return this as T;
        }

        //protected AppRuntimeBase(IDataSource data, bool showDrafts, ILog parentLog) : this()
        //{
        //    Init(data as IAppIdentity, showDrafts, parentLog);
        //    _data = data;
        //}

        #endregion


        #region Data & Cache

        public IDataSource Data => _data ?? (_data = new DataSource(Log).GetPublishing(this, showDrafts: ShowDrafts));
        private IDataSource _data;

        /// <summary>
        /// The cache-package if needed (mainly for export/import, where the full data is necessary)
        /// </summary>
        public AppState AppState => State.Get(this);


        #endregion


    }
}
