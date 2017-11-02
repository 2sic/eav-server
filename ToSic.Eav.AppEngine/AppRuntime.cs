using ToSic.Eav.App;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.DataSources;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps
{
    public class AppRuntime : AppBase
    {
        #region constructors
        public AppRuntime(int zoneId, int appId, Log parentLog) : base(zoneId, appId, parentLog) { }

        public AppRuntime(IApp app, Log parentLog) : base(app, parentLog) { }

        // Special constructor, should be used with care as there is no Zone!
        public AppRuntime(int appId, Log parentLog) :base (appId, parentLog) { }

        internal AppRuntime(IDataSource data, Log parentLog): base(data, parentLog) { }
        #endregion

        public EntityRuntime Entities => _entities ?? (_entities = new EntityRuntime(this, Log));
        private EntityRuntime _entities;

        public ContentTypeRuntime ContentTypes => _contentTypes ?? (_contentTypes = new ContentTypeRuntime(this, Log));
        private ContentTypeRuntime _contentTypes; 

        public QueryRuntime Queries => _queries ?? (_queries = new QueryRuntime(this, Log));
        private QueryRuntime _queries;

        public ZoneRuntime Zone => _zone ?? (_zone = new ZoneRuntime(ZoneId, Log));
        private ZoneRuntime _zone;

    }
}
