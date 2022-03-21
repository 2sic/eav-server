using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// The app management system - it's meant for modifying the app, not for reading the configuration. 
    /// Use other mechanisms if you only want to read content-types etc.
    /// </summary>
    public class AppManager: AppRuntimeBase<AppManager>
    {
        #region Constructors

        public AppManager(AppRuntimeDependencies dependencies) : this(dependencies, "Eav.AppMan")
        { }

        protected override void InitForDi()
        {
            Dependencies.AppRuntime.SetInit(r => r.InitWithState(AppState, ShowDrafts, Log));
            Dependencies.DbDataController.SetInit(c => c.Init(ZoneId, AppId, Log));
            Dependencies.EntitiesManager.SetInit(m => m.Init(this, Log));
            Dependencies.QueryManager.SetInit(m => m.Init(this, Log));
        }

        protected AppManager(AppRuntimeDependencies dependencies, string logName) : base(dependencies, logName) { }

        public AppManager Init(IAppIdentity app, ILog parentLog) => Init(app, true, parentLog);

        public AppManager Init(int appId, ILog parentLog) => Init(Dependencies.AppStates.IdentityOfApp(appId), true, parentLog);

        /// <summary>
        /// This is a very special overload to inject an app state without reloading.
        /// It's important because the app-manager must be able to help initialize an app, when it's not yet in the cache
        /// </summary>
        /// <returns></returns>
        public AppManager InitWithState(AppState appState, bool showDrafts, ILog parentLog)
        {
            AppState = appState;
            return Init(appState, showDrafts, parentLog);
        }

        #endregion

        #region Access the Runtime
        /// <summary>
        /// Read / Runtime system of the AppManager, to read data
        /// </summary>
        public AppRuntime Read => Dependencies.AppRuntime.Ready;

        #endregion

        /// <summary>
        /// Database controller / DB-Context
        /// </summary>
        internal DbDataController DataController =>  Dependencies.DbDataController.Ready;


        /// <summary>
        /// Storage system providing another interface
        /// </summary>
        internal IStorage Storage => DataController;

        /// <summary>
        /// The entity-management subsystem
        /// </summary>
        public EntitiesManager Entities => Dependencies.EntitiesManager.Ready;

        /// <summary>
        /// Queries Management Subsystem
        /// </summary>
        public QueryManager Queries => Dependencies.QueryManager.Ready;

        /// <summary>
        /// Content-Types Manager Subsystem
        /// </summary>
        public ContentTypeManager ContentTypes => _contentTypes ?? (_contentTypes = new ContentTypeManager().Init(this, Log));
        private ContentTypeManager  _contentTypes;

    }
}
