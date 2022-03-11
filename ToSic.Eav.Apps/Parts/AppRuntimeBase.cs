using System;
using ToSic.Eav.DataSources;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Root class for app runtime objects
    /// </summary>
    public abstract class AppRuntimeBase<T>: AppBase where T: AppRuntimeBase<T>
    {

        #region Constructor / DI

        public DataSourceFactory DataSourceFactory { get; }

        public bool ShowDrafts { get; private set; }

        // TODO: @STV Should become protected, but it's used so much this will take ca. 2-3 hours
        // The non-inheriting classes should get their services directly, not through this service provider
        public IServiceProvider ServiceProvider => DataSourceFactory.ServiceProvider;

        protected AppRuntimeBase(AppRuntimeDependencies dependencies, string logName): base(logName, new CodeRef())
        {
            Dependencies = dependencies;
            DataSourceFactory = dependencies.DataSourceFactory.Init(Log);
        }
        protected readonly AppRuntimeDependencies Dependencies;

        public T Init(IAppIdentity app, bool showDrafts, ILog parentLog)
        {
            Init(app, new CodeRef(), parentLog);
            // 2020-02-10 DataSourceFactory.Init(Log);
            // re-use data of parent if it's constructed from an app-manager
            if (app is AppManager parentIsAppManager) _data = parentIsAppManager.Data;
            ShowDrafts = showDrafts;
            return this as T;
        }

        #endregion



        #region Data & Cache

        public IDataSource Data => _data ?? (_data = DataSourceFactory.GetPublishing(this, showDrafts: ShowDrafts));
        private IDataSource _data;

        /// <summary>
        /// The cache-package if needed (mainly for export/import, where the full data is necessary)
        /// </summary>
        public AppState AppState
        {
            get => _appState ?? (_appState = Dependencies.AppStates/* State.*/.Get(this));
            protected set => _appState = value;
        }

        private AppState _appState;

        #endregion


    }
}
