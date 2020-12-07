using ToSic.Eav.Apps.Parts;
using ToSic.Eav.DataSources;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Apps
{
    /// <inheritdoc />
    /// <summary>
    /// Basic App-Reading System to access app data and read it
    /// </summary>
    public class AppRuntime : AppRuntimeBase<AppRuntime>
    {

        #region constructors

        public AppRuntime(DataSourceFactory dataSourceFactory, string logName = null) : base(dataSourceFactory, logName ?? "Eav.AppRt") {}

        /// <summary>
        /// Simple Override - to track if the init is being called everywhere
        /// </summary>
        public new AppRuntime Init(IAppIdentity app, bool showDrafts, ILog parentLog) 
            => base.Init(app, showDrafts, parentLog);

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
        public EntityRuntime Entities => _entities ?? (_entities = DataSourceFactory.ServiceProvider.Build< EntityRuntime>().Init(this, Log));
        private EntityRuntime _entities;

        /// <summary>
        /// Metadata runtime to get metadata from this app
        /// </summary>
        public MetadataRuntime Metadata => _metadata ?? (_metadata = DataSourceFactory.ServiceProvider.Build<MetadataRuntime>().Init(this, Log));
        private MetadataRuntime _metadata;

        /// <summary>
        /// ContentTypes runtime to get content types from this app
        /// </summary>
        public ContentTypeRuntime ContentTypes => _contentTypes ?? (_contentTypes = DataSourceFactory.ServiceProvider.Build<ContentTypeRuntime>().Init(this, Log));
        private ContentTypeRuntime _contentTypes; 

        /// <summary>
        /// Queries runtime to get queries of this app
        /// </summary>
        public QueryRuntime Queries => _queries ?? (_queries = DataSourceFactory.ServiceProvider.Build<QueryRuntime>().Init(this, Log));
        private QueryRuntime _queries;

        /// <summary>
        /// Zone runtime to get the zone of this app
        /// </summary>
        public ZoneRuntime Zone => _zone ?? (_zone = new ZoneRuntime().Init(ZoneId, Log));
        private ZoneRuntime _zone;

    }
}
