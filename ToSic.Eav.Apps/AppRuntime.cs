using ToSic.Eav.Apps.Parts;
using ToSic.Lib.DI;

namespace ToSic.Eav.Apps
{
    /// <inheritdoc />
    /// <summary>
    /// Basic App-Reading System to access app data and read it
    /// </summary>
    public class AppRuntime : AppRuntimeBase
    {

        #region constructors
        
        private readonly LazySvc<EntityRuntime> _entityRuntime;
        private readonly LazySvc<MetadataRuntime> _metadataRuntime;
        private readonly LazySvc<ContentTypeRuntime> _contentTypeRuntime;
        private readonly LazySvc<QueryRuntime> _queryRuntime;

        public AppRuntime(AppRuntimeDependencies dependencies,
            LazySvc<EntityRuntime> entityRuntime,
            LazySvc<MetadataRuntime> metadataRuntime,
            LazySvc<ContentTypeRuntime> contentTypeRuntime,
            LazySvc<QueryRuntime> queryRuntime,
            string logName = null) : base(dependencies,
            logName ?? "Eav.AppRt")
        {
            ConnectServices(
                _entityRuntime = entityRuntime.SetInit(r => r.ConnectTo(this)),
                _metadataRuntime = metadataRuntime.SetInit(r => r.ConnectTo(this)),
                _contentTypeRuntime = contentTypeRuntime.SetInit(r => r.ConnectTo(this)),
                _queryRuntime = queryRuntime.SetInit(r => r.ConnectTo(this))
            );
        }


        /// <summary>
        /// Simple Override - to track if the init is being called everywhere
        /// </summary>
        public AppRuntime Init(int appId, bool showDrafts) 
            => this.InitQ(Deps.AppStates.IdentityOfApp(appId), showDrafts);

        /// <summary>
        /// This is a very special overload to inject an app state without reloading.
        /// It's important because the app-manager must be able to help initialize an app, when it's not yet in the cache
        /// </summary>
        /// <returns></returns>
        protected internal AppRuntime InitWithState(AppState appState, bool showDrafts)
        {
            AppState = appState;
            return this.InitQ(appState, showDrafts);
        }


        #endregion

        /// <summary>
        /// Entities Runtime to get entities in this app
        /// </summary>
        public EntityRuntime Entities => _entityRuntime.Value;

        /// <summary>
        /// Metadata runtime to get metadata from this app
        /// </summary>
        public MetadataRuntime Metadata => _metadataRuntime.Value;

        /// <summary>
        /// ContentTypes runtime to get content types from this app
        /// </summary>
        public ContentTypeRuntime ContentTypes => _contentTypeRuntime.Value;

        /// <summary>
        /// Queries runtime to get queries of this app
        /// </summary>
        public QueryRuntime Queries => _queryRuntime.Value;


        ///// <summary>
        ///// Zone runtime to get the zone of this app
        ///// </summary>
        //public ZoneRuntime Zone => _zone ?? (_zone = new ZoneRuntime(Dependencies.AppStates).Init(ZoneId, Log));
        //private ZoneRuntime _zone;

    }
}
