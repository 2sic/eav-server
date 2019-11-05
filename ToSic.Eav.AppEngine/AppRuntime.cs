using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.DataSources;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps
{
    /// <inheritdoc />
    /// <summary>
    /// Basic App-Reading System to access app data and read it
    /// </summary>
    public class AppRuntime : AppBase
    {
        #region constructors
        public AppRuntime(int zoneId, int appId, ILog parentLog) : base(zoneId, appId, parentLog) { }

        public AppRuntime(IAppIdentity app, ILog parentLog) : base(app, parentLog) { }

        /// <summary>
        ///  Special constructor, should be used with care as there is no Zone!
        /// </summary>
        public AppRuntime(int appId, ILog parentLog) :base (appId, parentLog) { }

        internal AppRuntime(IDataSource data, ILog parentLog): base(data, parentLog) { }
        #endregion

        /// <summary>
        /// Entities Runtime to get entities in this app
        /// </summary>
        public EntityRuntime Entities => _entities ?? (_entities = new EntityRuntime(this, Log));
        private EntityRuntime _entities;

        /// <summary>
        /// Metadata runtime to get metadata from this app
        /// </summary>
        public MetadataRuntime Metadata => _metadata ?? (_metadata = new MetadataRuntime(this, Log));
        private MetadataRuntime _metadata;

        /// <summary>
        /// ContentTypes runtime to get content types from this app
        /// </summary>
        public ContentTypeRuntime ContentTypes => _contentTypes ?? (_contentTypes = new ContentTypeRuntime(this, Log));
        private ContentTypeRuntime _contentTypes; 

        /// <summary>
        /// Queries runtime to get queries of this app
        /// </summary>
        public QueryRuntime Queries => _queries ?? (_queries = new QueryRuntime(this, Log));
        private QueryRuntime _queries;

        /// <summary>
        /// Zone runtime to get the zone of this app
        /// </summary>
        public ZoneRuntime Zone => _zone ?? (_zone = new ZoneRuntime(ZoneId, Log));
        private ZoneRuntime _zone;

    }
}
