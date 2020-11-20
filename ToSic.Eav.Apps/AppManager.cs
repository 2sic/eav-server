using ToSic.Eav.Apps.Parts;
using ToSic.Eav.DataSources;
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
        #region Constructors

        public AppManager(DataSourceFactory dataSourceFactory) : base(dataSourceFactory, "Eav.AppMan") { }

        protected AppManager(DataSourceFactory dataSourceFactory, string logName) : base(dataSourceFactory, logName) { }

        public AppManager Init(IAppIdentity app, ILog parentLog) => Init(app, true, parentLog);

        public AppManager Init(int appId, ILog parentLog) => Init(State.Identity(null, appId), true, parentLog);

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
        public AppRuntime Read => _read ?? (_read = ServiceProvider.Build<AppRuntime>().InitWithState(AppState, ShowDrafts, Log));
        private AppRuntime _read;
        #endregion

        /// <summary>
        /// Database controller / DB-Context
        /// </summary>
        internal DbDataController DataController 
            => _eavContext ?? (_eavContext = ServiceProvider.Build<DbDataController>().Init(ZoneId, AppId, Log));
        private DbDataController _eavContext;

        /// <summary>
        /// Storage system providing another interface
        /// </summary>
        internal IStorage Storage => DataController;

        /// <summary>
        /// The entity-management subsystem
        /// </summary>
        public EntitiesManager Entities => _entities ?? (_entities = ServiceProvider.Build<EntitiesManager>().Init(this, Log));
        private EntitiesManager _entities;

        /// <summary>
        /// Queries Management Subsystem
        /// </summary>
        public QueryManager Queries => _queries ?? (_queries = new QueryManager().Init(this, Log));
        private QueryManager _queries;

        /// <summary>
        /// Content-Types Manager Subsystem
        /// </summary>
        public ContentTypeManager ContentTypes => _contentTypes ?? (_contentTypes = new ContentTypeManager().Init(this, Log));
        private ContentTypeManager  _contentTypes;

    }
}
