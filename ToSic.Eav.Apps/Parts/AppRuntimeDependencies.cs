using ToSic.Eav.DataSources;
using ToSic.Eav.Documentation;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps.Parts
{
    [PrivateApi]
    public class AppRuntimeDependencies
    {
        public DataSourceFactory DataSourceFactory { get; }
        public IAppStates AppStates { get; }
        public ZoneRuntime ZoneRuntime { get; }
        public LazyInit<EntityRuntime> EntityRuntime { get; }
        public LazyInit<MetadataRuntime> MetadataRuntime { get; }
        public LazyInit<ContentTypeRuntime> ContentTypeRuntime { get; }
        public LazyInit<QueryRuntime> QueryRuntime { get; }
        public LazyInit<AppRuntime> AppRuntime { get; }
        public LazyInit<DbDataController> DbDataController { get; }
        public LazyInit<EntitiesManager> EntitiesManager { get; }
        public LazyInit<QueryManager> QueryManager { get; }

        public AppRuntimeDependencies(DataSourceFactory dataSourceFactory, 
            IAppStates appStates, 
            ZoneRuntime zoneRuntime, 
            LazyInit<EntityRuntime> entityRuntime,
            LazyInit<MetadataRuntime> metadataRuntime,
            LazyInit<ContentTypeRuntime> contentTypeRuntime,
            LazyInit<QueryRuntime> queryRuntime,
            LazyInit<AppRuntime> appRuntime,
            LazyInit<DbDataController> dbDataController,
            LazyInit<EntitiesManager> entitiesManager,
            LazyInit<QueryManager> queryManager)
        {
            DataSourceFactory = dataSourceFactory;
            AppStates = appStates;
            ZoneRuntime = zoneRuntime;
            EntityRuntime = entityRuntime;
            MetadataRuntime = metadataRuntime;
            ContentTypeRuntime = contentTypeRuntime;
            QueryRuntime = queryRuntime;
            AppRuntime = appRuntime;
            DbDataController = dbDataController;
            EntitiesManager = entitiesManager;
            QueryManager = queryManager;
        }

    }
}
