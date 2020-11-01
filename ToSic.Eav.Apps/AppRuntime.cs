using ToSic.Eav.Apps.Parts;
using ToSic.Eav.DataSources;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps
{
    /// <inheritdoc />
    /// <summary>
    /// Basic App-Reading System to access app data and read it
    /// </summary>
    public class AppRuntime : AppRuntimeBase
    {

        #region constructors

        public AppRuntime(IAppIdentity app, bool showDrafts, ILog parentLog) : base(app, showDrafts, parentLog) { }

        /// <summary>
        ///  Special constructor, should be used with care as there is no Zone!
        /// </summary>
        public AppRuntime(int appId, bool showDrafts, ILog parentLog) 
            :this (State.Identity(null, appId), showDrafts, parentLog) { }

        internal AppRuntime(IDataSource data, bool showDrafts, ILog parentLog): base(data, showDrafts, parentLog) { }
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
        public ContentTypeRuntime ContentTypes => _contentTypes ?? (_contentTypes = new ContentTypeRuntime().Init(this, Log));
        private ContentTypeRuntime _contentTypes; 

        /// <summary>
        /// Queries runtime to get queries of this app
        /// </summary>
        public QueryRuntime Queries => _queries ?? (_queries = new QueryRuntime().Init(this, Log));
        private QueryRuntime _queries;

        /// <summary>
        /// Zone runtime to get the zone of this app
        /// </summary>
        public ZoneRuntime Zone => _zone ?? (_zone = new ZoneRuntime(ZoneId, Log));
        private ZoneRuntime _zone;

    }
}
