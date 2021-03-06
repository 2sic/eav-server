﻿using System;
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

        public IServiceProvider ServiceProvider => DataSourceFactory.ServiceProvider;

        protected AppRuntimeBase(DataSourceFactory dataSourceFactory, string logName): base(logName, new CodeRef())
        {
            DataSourceFactory = dataSourceFactory.Init(Log);
        }

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
            get => _appState ?? (_appState = State.Get(this));
            protected set => _appState = value;
        }

        private AppState _appState;

        #endregion


    }
}
