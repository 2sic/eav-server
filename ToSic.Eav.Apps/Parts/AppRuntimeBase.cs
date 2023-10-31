using ToSic.Eav.Apps.AppSys;
using ToSic.Eav.Services;
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
            public IDataSourcesService DataSourceFactory { get; }
            public IAppStates AppStates { get; }

            public MyServices(
                IDataSourcesService dataSourceFactory,
                IAppStates appStates
            ) => ConnectServices(
                DataSourceFactory = dataSourceFactory,
                AppStates = appStates
            );
        }


        public bool? ShowDrafts { get; private set; }

        protected AppRuntimeBase(MyServices services, string logName): base(services, logName)
        {
        }

        protected AppRuntimeBase(MyServicesBase<MyServices> services, string logName): base(services, logName)
        {
        }

        internal void InitInternal(IAppIdentity app, bool? showDrafts)
        {
            InitAppBaseIds(app);
            ShowDrafts = showDrafts;
        }

        #endregion



        #region Data & Cache

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


        internal IDataSourcesService DsfWip => Services.DataSourceFactory;
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

        public static IAppWorkCtx GetContextWip<T>(this T appRuntime) where T : AppRuntimeBase
        {
            return new AppWorkCtx(appRuntime.DsfWip, appRuntime.AppState, appRuntime.ShowDrafts);
        }
    }
}
