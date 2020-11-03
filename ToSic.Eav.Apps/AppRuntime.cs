using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps
{
    /// <inheritdoc />
    /// <summary>
    /// Basic App-Reading System to access app data and read it
    /// </summary>
    public class AppRuntime : AppRuntimeBase<AppRuntime>
    {

        #region constructors
        public AppRuntime() : base("Eav.AppRt") {}

        public AppRuntime(string logName) : base(logName) {}

        /// <summary>
        /// Simple Override - to track if the init is being called everywhere
        /// </summary>
        public new AppRuntime Init(IAppIdentity app, bool showDrafts, ILog parentLog) 
            => base.Init(app, showDrafts, parentLog);

        #endregion

        #region DataSourceFactory
        public DataSource DataSourceFactory => _dsFactory ?? (_dsFactory = new DataSource(Log));
        private DataSource _dsFactory;

        #endregion 

        /// <summary>
        /// Entities Runtime to get entities in this app
        /// </summary>
        public EntityRuntime Entities => _entities ?? (_entities = new EntityRuntime().Init(this, Log));
        private EntityRuntime _entities;

        /// <summary>
        /// Metadata runtime to get metadata from this app
        /// </summary>
        public MetadataRuntime Metadata => _metadata ?? (_metadata = new MetadataRuntime().Init(this, Log));
        private MetadataRuntime _metadata;

        /// <summary>
        /// ContentTypes runtime to get content types from this app
        /// </summary>
        public ContentTypeRuntime ContentTypes => _contentTypes ?? (_contentTypes = Factory.Resolve<ContentTypeRuntime>().Init(this, Log));
        private ContentTypeRuntime _contentTypes; 

        /// <summary>
        /// Queries runtime to get queries of this app
        /// </summary>
        public QueryRuntime Queries => _queries ?? (_queries = new QueryRuntime().Init(this, Log));
        private QueryRuntime _queries;

        /// <summary>
        /// Zone runtime to get the zone of this app
        /// </summary>
        public ZoneRuntime Zone => _zone ?? (_zone = new ZoneRuntime().Init(ZoneId, Log));
        private ZoneRuntime _zone;

    }
}
