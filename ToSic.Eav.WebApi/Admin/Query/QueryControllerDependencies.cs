using System;
using ToSic.Eav.Apps;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.DataSources;
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
        public LazySvc<AppManager> AppManagerLazy { get; }
        /// <summary>
        /// The lazy reader should only be used in the Definition - it's important that it's a new object
        /// when used, to ensure it has the changes previously saved
        /// </summary>
        public LazySvc<AppRuntime> AppReaderLazy { get; }
        public QueryBuilder QueryBuilder { get; }
        public LazySvc<ConvertToEavLight> EntToDicLazy { get; }
        public LazySvc<QueryInfo> QueryInfoLazy { get; }
        public LazySvc<DataSourceCatalog> DataSourceCatalogLazy { get; }
        public Generator<JsonSerializer> JsonSerializer { get; }
        public Generator<PassThrough> PassThrough { get; }

        public QueryControllerDependencies(LazySvc<AppManager> appManagerLazy,
            LazySvc<AppRuntime> appReaderLazy,
            QueryBuilder queryBuilder,
            LazySvc<ConvertToEavLight> entToDicLazy,
            LazySvc<QueryInfo> queryInfoLazy,
            LazySvc<DataSourceCatalog> dataSourceCatalogLazy,
            Generator<JsonSerializer> jsonSerializer,
            Generator<PassThrough> passThrough)
        {
            AddToLogQueue(
                AppManagerLazy = appManagerLazy,
                AppReaderLazy = appReaderLazy,
                QueryBuilder = queryBuilder,
                EntToDicLazy = entToDicLazy,
                QueryInfoLazy = queryInfoLazy,
                DataSourceCatalogLazy = dataSourceCatalogLazy,
                JsonSerializer = jsonSerializer,
                PassThrough = passThrough
            );
        }
    }
}