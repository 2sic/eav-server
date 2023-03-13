using ToSic.Eav.DataSources;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Root class for app runtime objects
    /// </summary>
    public abstract class AppRuntimeBase : AppBase<AppRuntimeBase.MyServices>
    {

        #region Constructor / DI

        public class MyServices : MyServicesBase
        {
            public DataSourceFactory DataSourceFactory { get; }
            public IAppStates AppStates { get; }
            public ZoneRuntime ZoneRuntime { get; }

            public MyServices(
                DataSourceFactory dataSourceFactory,
                IAppStates appStates,
                ZoneRuntime zoneRuntime
            ) => ConnectServices(
                DataSourceFactory = dataSourceFactory,
                AppStates = appStates,
                ZoneRuntime = zoneRuntime
            );
        }


        public bool? ShowDrafts { get; private set; }

        protected AppRuntimeBase(MyServices services, string logName): base(services, logName)
        {
        }
        //protected readonly AppRuntimeServices Deps;
        protected AppRuntimeBase(MyServicesBase<MyServices> services, string logName): base(services, logName)
        {
        }

        internal void InitInternal(IAppIdentity app, bool? showDrafts)
        {
            Init(app);
            // re-use data of parent if it's constructed from an app-manager
            if (app is AppManager parentIsAppManager) _data = parentIsAppManager.Data;
            ShowDrafts = showDrafts;
        }

        #endregion



        #region Data & Cache

        public IDataSource Data => _data ?? (_data = Services.DataSourceFactory.CreateDefault(appIdentity: this, showDrafts: ShowDrafts));
        private IDataSource _data;
        

        /// <summary>
        /// The cache-package if needed (mainly for export/import, where the full data is necessary)
        /// </summary>
        public AppState AppState
        {
            get => _appState ?? (_appState = Services.AppStates.Get(this));
            protected set => _appState = value;
        }

        private AppState _appState;

        #endregion

    }

    public static class AppRuntimeExtensions
    {
        public static T InitQ<T>(this T parent, IAppIdentity app, bool? showDrafts) where T : AppRuntimeBase
        {
            parent.InitInternal(app, showDrafts);
            return parent;
        }
        public static T InitQ<T>(this T parent, IAppIdentity app) where T : AppRuntimeBase
        {
            parent.InitInternal(app, null);
            return parent;
        }
    }
}
