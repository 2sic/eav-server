using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.DI;

namespace ToSic.Eav.Apps.Parts
{
    public class QueryRuntime: PartOf<AppRuntime/*, QueryRuntime*/>
    {
        private readonly GeneratorLog<Eav.DataSources.Queries.QueryManager> _queryManager;
        public QueryRuntime(GeneratorLog<Eav.DataSources.Queries.QueryManager> queryManager) : base("RT.Query") 
            => ConnectServices(_queryManager = queryManager);


        /// <summary>
        /// Get a query definition from the current app
        /// </summary>
        /// <param name="queryId"></param>
        /// <returns></returns>
        public QueryDefinition Get(int queryId)
        {
            var queryMan = _queryManager.New();
            return new QueryDefinition(queryMan.GetQueryEntity(queryId, Parent.AppState),
                Parent.AppId, Log);
        }

    }
}
