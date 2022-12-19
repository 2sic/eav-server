using ToSic.Eav.DataSources;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Root class for app runtime objects
    /// </summary>
    public abstract class AppRuntimeBase : AppBase
    {

        #region Constructor / DI

        public bool ShowDrafts { get; private set; }

        protected AppRuntimeBase(AppRuntimeDependencies dependencies, string logName): base(logName, new CodeRef())
        {
            Dependencies = dependencies.SetLog(Log);
        }
        protected readonly AppRuntimeDependencies Dependencies;

        internal void InitInternal(IAppIdentity app, bool showDrafts)
        {
            Init(app);
            // re-use data of parent if it's constructed from an app-manager
            if (app is AppManager parentIsAppManager) _data = parentIsAppManager.Data;
            ShowDrafts = showDrafts;
        }

        #endregion



        #region Data & Cache

        public IDataSource Data => _data ?? (_data = Dependencies.DataSourceFactory.GetPublishing(this, showDrafts: ShowDrafts));
        private IDataSource _data;
        

        /// <summary>
        /// The cache-package if needed (mainly for export/import, where the full data is necessary)
        /// </summary>
        public AppState AppState
        {
            get => _appState ?? (_appState = Dependencies.AppStates.Get(this));
            protected set => _appState = value;
        }

        private AppState _appState;

        #endregion

    }

    public static class AppRuntimeExtensions
    {
        public static T InitQ<T>(this T parent, IAppIdentity app, bool showDrafts) where T : AppRuntimeBase
        {
            parent.InitInternal(app, showDrafts);
            return parent;
        }
    }
}
