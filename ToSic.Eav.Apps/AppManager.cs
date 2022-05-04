using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// The app management system - it's meant for modifying the app, not for reading the configuration. 
    /// Use other mechanisms if you only want to read content-types etc.
    /// </summary>
    public class AppManager: AppRuntimeBase<AppManager>
    {
        private readonly LazyInit<AppRuntime> _appRuntime;
        private readonly LazyInit<DbDataController> _dbDataController;
        private readonly LazyInit<EntitiesManager> _entitiesManager;
        private readonly LazyInit<QueryManager> _queryManager;

        #region Constructors

        protected AppManager(AppRuntimeDependencies dependencies,
            LazyInit<AppRuntime> appRuntime,
            LazyInit<DbDataController> dbDataController,
            LazyInit<EntitiesManager> entitiesManager,
            LazyInit<QueryManager> queryManager,
            string logName
            ) : base(dependencies, logName)
        {
            _appRuntime = appRuntime.SetInit(r => r.InitWithState(AppState, ShowDrafts, Log));
            _dbDataController = dbDataController.SetInit(c => c.Init(AppState, Log));
            _entitiesManager = entitiesManager.SetInit(m => m.Init(this, Log));
            _queryManager = queryManager.SetInit(m => m.Init(this, Log));
        }

        public AppManager(AppRuntimeDependencies dependencies,
            LazyInit<AppRuntime> appRuntime,
            LazyInit<DbDataController> dbDataController,
            LazyInit<EntitiesManager> entitiesManager,
            LazyInit<QueryManager> queryManager
            ) : this(dependencies, appRuntime, dbDataController, entitiesManager, queryManager, "Eav.AppMan")
        { }

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
        public AppRuntime Read => _appRuntime.Ready;

        #endregion

        /// <summary>
        /// Database controller / DB-Context
        /// </summary>
        internal DbDataController DataController =>  _dbDataController.Ready;


        /// <summary>
        /// Storage system providing another interface
        /// </summary>
        internal IStorage Storage => DataController;

        /// <summary>
        /// The entity-management subsystem
        /// </summary>
        public EntitiesManager Entities => _entitiesManager.Ready;

        /// <summary>
        /// Queries Management Subsystem
        /// </summary>
        public QueryManager Queries => _queryManager.Ready;

        /// <summary>
        /// Content-Types Manager Subsystem
        /// </summary>
        public ContentTypeManager ContentTypes => _contentTypes ?? (_contentTypes = new ContentTypeManager().Init(this, Log));
        private ContentTypeManager  _contentTypes;

    }
}
