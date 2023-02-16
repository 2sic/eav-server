using ToSic.Eav.DataSources;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Root class for app runtime objects
    /// </summary>
    public abstract class AppRuntimeBase : AppBase
    {

        #region Constructor / DI

        public bool ShowDrafts { get; private set; }

        protected AppRuntimeBase(AppRuntimeServices services, string logName): base(logName)
        {
            ConnectServices(
                Deps = services
            );
        }
        protected readonly AppRuntimeServices Deps;

        internal void InitInternal(IAppIdentity app, bool showDrafts)
        {
            Init(app);
            // re-use data of parent if it's constructed from an app-manager
            if (app is AppManager parentIsAppManager) _data = parentIsAppManager.Data;
            ShowDrafts = showDrafts;
        }

        #endregion



        #region Data & Cache

        public IDataSource Data => _data ?? (_data = Deps.DataSourceFactory.GetPublishing(this, showDrafts: ShowDrafts));
        private IDataSource _data;
        

        /// <summary>
        /// The cache-package if needed (mainly for export/import, where the full data is necessary)
        /// </summary>
        public AppState AppState
        {
            get => _appState ?? (_appState = Deps.AppStates.Get(this));
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
