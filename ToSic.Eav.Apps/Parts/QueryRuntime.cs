﻿using ToSic.Eav.DataSource.Query;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Parts
{
    public class QueryRuntime: PartOf<AppRuntime>
    {
        private readonly LazySvc<QueryDefinitionBuilder> _queryDefBuilder;
        private readonly Generator<DataSource.Query.QueryManager> _queryManager;
        public QueryRuntime(Generator<DataSource.Query.QueryManager> queryManager, LazySvc<QueryDefinitionBuilder> queryDefBuilder) : base("RT.Query")
        {
            ConnectServices(
                _queryManager = queryManager,
                _queryDefBuilder = queryDefBuilder
            );
        }


        /// <summary>
        /// Get a query definition from the current app
        /// </summary>
        /// <param name="queryId"></param>
        /// <returns></returns>
        public QueryDefinition Get(int queryId) => Log.Func($"{nameof(queryId)}:{queryId}", () =>
            _queryDefBuilder.Value.Create(_queryManager.New().GetQueryEntity(queryId, Parent.AppState), Parent.AppId));

    }
}
