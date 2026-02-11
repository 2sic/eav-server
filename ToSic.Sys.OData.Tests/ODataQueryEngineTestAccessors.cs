using ToSic.Sys.OData.Ast;

namespace ToSic.Sys.OData.Tests;


internal static class UriQueryParserTac
{
    public static ODataQuery ToQueryTac(this IDictionary<string, string> queryOptions)
        => queryOptions.ToQuery();
}