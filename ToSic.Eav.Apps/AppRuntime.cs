using ToSic.Eav.Apps.Parts;
using ToSic.Eav.DI;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps
{
    /// <inheritdoc />
    /// <summary>
    /// Basic App-Reading System to access app data and read it
    /// </summary>
    public class AppRuntime : AppRuntimeBase<AppRuntime>
    {

        #region constructors
        
        private readonly LazyInit<EntityRuntime> _entityRuntime;
        private readonly LazyInit<MetadataRuntime> _metadataRuntime;
        private readonly LazyInit<ContentTypeRuntime> _contentTypeRuntime;
        private readonly LazyInit<QueryRuntime> _queryRuntime;

        public AppRuntime(AppRuntimeDependencies dependencies,
            LazyInit<EntityRuntime> entityRuntime,
            LazyInit<MetadataRuntime> metadataRuntime,
            LazyInit<ContentTypeRuntime> contentTypeRuntime,
            LazyInit<QueryRuntime> queryRuntime,
            string logName = null) : base(dependencies,
            logName ?? "Eav.AppRt")
        {
            _entityRuntime = entityRuntime.SetInit(r => r.Init(this, Log));
            _metadataRuntime = metadataRuntime.SetInit(r => r.Init(this, Log));
            _contentTypeRuntime = contentTypeRuntime.SetInit(r => r.Init(this, Log));
            _queryRuntime = queryRuntime.SetInit(r => r.Init(this, Log));
        }

        /// <summary>
        /// Simple Init
        /// </summary>
        public new AppRuntime Init(IAppIdentity app, bool showDrafts, ILog parentLog) 
            => base.Init(app, showDrafts, parentLog);
        
        /// <summary>
        /// Simple Override - to track if the init is being called everywhere
        /// </summary>
        public AppRuntime Init(int appId, bool showDrafts, ILog parentLog) 
            => base.Init(Dependencies.AppStates.IdentityOfApp(appId), showDrafts, parentLog);

        /// <summary>
        /// This is a very special overload to inject an app state without reloading.
        /// It's important because the app-manager must be able to help initialize an app, when it's not yet in the cache
        /// </summary>
        /// <returns></returns>
        protected internal AppRuntime InitWithState(AppState appState, bool showDrafts, ILog parentLog)
        {
            AppState = appState;
            return base.Init(appState, showDrafts, parentLog);
        }


        #endregion

        /// <summary>
        /// Entities Runtime to get entities in this app
        /// </summary>
        public EntityRuntime Entities => _entityRuntime.Ready;

        /// <summary>
        /// Metadata runtime to get metadata from this app
        /// </summary>
        public MetadataRuntime Metadata => _metadataRuntime.Ready;

        /// <summary>
        /// ContentTypes runtime to get content types from this app
        /// </summary>
        public ContentTypeRuntime ContentTypes => _contentTypeRuntime.Ready;

        /// <summary>
        /// Queries runtime to get queries of this app
        /// </summary>
        public QueryRuntime Queries => _queryRuntime.Ready;


        ///// <summary>
        ///// Zone runtime to get the zone of this app
        ///// </summary>
        //public ZoneRuntime Zone => _zone ?? (_zone = new ZoneRuntime(Dependencies.AppStates).Init(ZoneId, Log));
        //private ZoneRuntime _zone;

    }
}
