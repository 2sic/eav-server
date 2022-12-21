using System;
using ToSic.Eav.Apps;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.DataSources.Catalog;
using ToSic.Eav.DataSources.Debug;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.ImportExport.Json;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.WebApi.Admin.Query
{
    public class QueryControllerDependencies: ServiceDependencies
    {
        public Lazy<AppManager> AppManagerLazy { get; }
        /// <summary>
        /// The lazy reader should only be used in the Definition - it's important that it's a new object
        /// when used, to ensure it has the changes previously saved
        /// </summary>
        public LazyInitLog<AppRuntime> AppReaderLazy { get; }
        public QueryBuilder QueryBuilder { get; }
        public LazyInitLog<ConvertToEavLight> EntToDicLazy { get; }
        public LazyInitLog<QueryInfo> QueryInfoLazy { get; }
        public LazyInitLog<DataSourceCatalog> DataSourceCatalogLazy { get; }
        public Generator<JsonSerializer> JsonSerializer { get; }

        public QueryControllerDependencies(Lazy<AppManager> appManagerLazy,
            LazyInitLog<AppRuntime> appReaderLazy,
            QueryBuilder queryBuilder,
            LazyInitLog<ConvertToEavLight> entToDicLazy,
            LazyInitLog<QueryInfo> queryInfoLazy,
            LazyInitLog<DataSourceCatalog> dataSourceCatalogLazy,
            Generator<JsonSerializer> jsonSerializer)
        {
            AppManagerLazy = appManagerLazy;
            AppReaderLazy = appReaderLazy;
            QueryBuilder = queryBuilder;
            EntToDicLazy = entToDicLazy;
            QueryInfoLazy = queryInfoLazy;
            DataSourceCatalogLazy = dataSourceCatalogLazy;
            JsonSerializer = jsonSerializer;
        }
    }
}