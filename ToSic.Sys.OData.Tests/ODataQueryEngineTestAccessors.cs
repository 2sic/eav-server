using ToSic.Eav.DataSource;
using ToSic.Eav.DataSources;
using ToSic.Sys.OData.Ast;

namespace ToSic.Sys.OData.Tests;

internal static class ODataQueryEngineTestAccessors
{
    public static QueryExecutionResult ExecuteTac(this ODataQueryEngine engine, IDataSource root, ODataQuery oDataQuery)
        => engine.Execute(root, oDataQuery);

}

internal static class UriQueryParserTac
{
    public static ODataQuery Parse(IDictionary<string, string> queryOptions)
        => UriQueryParser.Parse(queryOptions);
}