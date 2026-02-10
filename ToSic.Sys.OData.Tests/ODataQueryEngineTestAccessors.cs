using ToSic.Sys.OData.Ast;

namespace ToSic.Sys.OData.Tests;


internal static class UriQueryParserTac
{
    public static ODataQuery Parse(IDictionary<string, string> queryOptions)
        => UriQueryParser.Parse(queryOptions);
}