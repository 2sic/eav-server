using System;
using ToSic.Eav.Apps;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.DataSources.Catalog;
using ToSic.Eav.DataSources.Debug;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.DI;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.WebApi.Admin.Query
{
    public class QueryControllerDependencies
    {
        public Lazy<AppManager> AppManagerLazy { get; }
        /// <summary>
        /// The lazy reader should only be used in the Definition - it's important that it's a new object
        /// when used, to ensure it has the changes previously saved
        /// </summary>
        public Lazy<AppRuntime> AppReaderLazy { get; }
        public QueryBuilder QueryBuilder { get; }
        public Lazy<ConvertToEavLight> EntToDicLazy { get; }
        public Lazy<QueryInfo> QueryInfoLazy { get; }
        public Lazy<DataSourceCatalog> DataSourceCatalogLazy { get; }
        public Generator<JsonSerializer> JsonSerializer { get; }

        public QueryControllerDependencies(Lazy<AppManager> appManagerLazy,
            Lazy<AppRuntime> appReaderLazy,
            QueryBuilder queryBuilder,
            Lazy<ConvertToEavLight> entToDicLazy,
            Lazy<QueryInfo> queryInfoLazy,
            Lazy<DataSourceCatalog> dataSourceCatalogLazy,
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