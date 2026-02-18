using ToSic.Sys.OData;
using ToSic.Sys.OData.Ast;

namespace ToSic.Eav.DataSource.OData;

internal static class ODataQueryEngineTestAccessors
{
    public static QueryExecutionResult ExecuteTac(this ODataQueryEngine engine, IDataSource root, ODataQuery oDataQuery)
        => engine.Execute(root, oDataQuery);

}

internal static class UriQueryParserTac
{
    public static ODataQuery ToQueryTac(this IDictionary<string, string> queryOptions)
        => queryOptions.ToQuery();
}