using ToSic.Eav.Apps.Parts;
using ToSic.Lib.DI;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Repository.Efc;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// The app management system - it's meant for modifying the app, not for reading the configuration. 
    /// Use other mechanisms if you only want to read content-types etc.
    /// </summary>
    public class AppManager: AppRuntimeBase
    {
        #region Constructors

        public class Dependencies: ServiceDependencies<AppRuntimeDependencies>
        {
            public LazySvc<AppRuntime> AppRuntime { get; }
            public LazySvc<DbDataController> DbDataController { get; }
            public LazySvc<EntitiesManager> EntitiesManager { get; }
            public LazySvc<QueryManager> QueryManager { get; }
            public LazySvc<ContentTypeManager> ContentTypeManager { get; }

            public Dependencies(AppRuntimeDependencies rootDependencies,
                LazySvc<AppRuntime> appRuntime,
                LazySvc<DbDataController> dbDataController,
                LazySvc<EntitiesManager> entitiesManager,
                LazySvc<QueryManager> queryManager,
                LazySvc<ContentTypeManager> contentTypeManager
            ) : base(rootDependencies)
            {
                AddToLogQueue(
                    AppRuntime = appRuntime,
                    DbDataController = dbDataController,
                    EntitiesManager = entitiesManager,
                    QueryManager = queryManager,
                    ContentTypeManager = contentTypeManager
                );
            }
        }

        protected AppManager(Dependencies dependencies, string logName) : base(dependencies.RootDependencies, logName)
        {
            _deps = dependencies.SetLog(Log);
            _deps.AppRuntime.SetInit(r => r.InitWithState(AppState, ShowDrafts));
            _deps.DbDataController.SetInit(c =>
            {
                // TODO: STV this is a bit of a hack, but it's the only way to get the app-state into the DbDataController
                // find what is wrong with AppState
                if ((base._Deps.AppStates as AppStates)?.IsCached(this) ?? false)
                    c.Init(AppState);
                else
                    c.Init(ZoneId, AppId);
            });
            _deps.EntitiesManager.SetInit(m => m.ConnectTo(this));
            _deps.QueryManager.SetInit(m => m.ConnectTo(this));
            _deps.ContentTypeManager.SetInit(ct => ct.ConnectTo(this));
        }
        private readonly Dependencies _deps;

        public AppManager(Dependencies dependencies) : this(dependencies, "Eav.AppMan")
        { }

        public new AppManager Init(IAppIdentity app) => this.InitQ(app, true);

        public AppManager Init(int appId) => this.InitQ(_Deps.AppStates.IdentityOfApp(appId), true);

        /// <summary>
        /// This is a very special overload to inject an app state without reloading.
        /// It's important because the app-manager must be able to help initialize an app, when it's not yet in the cache
        /// </summary>
        /// <returns></returns>
        public AppManager InitWithState(AppState appState, bool showDrafts)
        {
            AppState = appState;
            return this.InitQ(appState, showDrafts);
        }

        #endregion

        #region Access the Runtime

        /// <summary>
        /// Read / Runtime system of the AppManager, to read data
        /// </summary>
        public AppRuntime Read => _deps.AppRuntime.Value;

        #endregion

        /// <summary>
        /// Database controller / DB-Context
        /// </summary>
        internal DbDataController DataController => _deps.DbDataController.Value;


        /// <summary>
        /// Storage system providing another interface
        /// </summary>
        internal IStorage Storage => DataController;

        /// <summary>
        /// The entity-management subsystem
        /// </summary>
        public EntitiesManager Entities => _deps.EntitiesManager.Value;

        /// <summary>
        /// Queries Management Subsystem
        /// </summary>
        public QueryManager Queries => _deps.QueryManager.Value;

        /// <summary>
        /// Content-Types Manager Subsystem
        /// </summary>
        public ContentTypeManager ContentTypes => _deps.ContentTypeManager.Value;

    }
}
