using ToSic.Eav.DataSources.Queries;

namespace ToSic.Eav.Apps.Parts
{
    public class QueryRuntime: PartOf<AppRuntime, QueryRuntime>
    {
        public QueryRuntime() : base("RT.Query") { }


        /// <summary>
        /// Get a query definition from the current app
        /// </summary>
        /// <param name="queryId"></param>
        /// <returns></returns>
        public QueryDefinition Get(int queryId)
        {
            var queryMan = new Eav.DataSources.Queries.QueryManager(Parent.DataSourceFactory).Init(Log);
            return new QueryDefinition(queryMan.GetQueryEntity(queryId, Parent.AppState),
                Parent.AppId, Log);
        }

    }
}
