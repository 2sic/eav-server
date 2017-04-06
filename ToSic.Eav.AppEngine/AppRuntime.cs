using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.Apps
{
    public class AppRuntime : AppBase
    {
        #region constructors
        public AppRuntime(int zoneId, int appId) : base(zoneId, appId) { }

        public AppRuntime(IApp app) : base(app) { }

        // Special constructor, should be used with care as there is no Zone!
        public AppRuntime(int appId) :base (appId) { }

        internal AppRuntime(IDataSource data): base(data) { }
        #endregion

        public EntityRuntime Entities => _entities ?? (_entities = new EntityRuntime(this));
        private EntityRuntime _entities;

        public ContentTypeRuntime ContentTypes => _contentTypes ?? (_contentTypes = new ContentTypeRuntime(this));
        private ContentTypeRuntime _contentTypes; 

        public QueryRuntime Queries => _queries ?? (_queries = new QueryRuntime(this));
        private QueryRuntime _queries; 



    }
}
