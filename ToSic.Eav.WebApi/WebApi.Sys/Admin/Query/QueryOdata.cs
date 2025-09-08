using System.Collections.ObjectModel;
using ToSic.Eav.DataSource;
using ToSic.Sys.OData;

namespace ToSic.Eav.WebApi.Sys.Admin.Query;

internal class QueryODataParams
{
    public QueryODataParams(IDataSourceConfiguration? config)
    {
        if (config == null)
            return;

        var extraParams = config.Parse(ODataParams);

        SystemQueryOptions = new SystemQueryOptions(
            Select: SystemQueryOptionsParser.ParseSelect(Get(extraParams, ODataConstants.SelectParamName)),
            Filter: Get(extraParams, ODataConstants.FilterParamName),
            OrderBy: Get(extraParams, ODataConstants.OrderByParamName),
            Top: AsInt(Get(extraParams, ODataConstants.TopParamName)), // long in OData spec, but int should be enough for us
            Skip: AsInt(Get(extraParams, ODataConstants.SkipParamName)), // long in OData spec, but int should be enough for us
            Count: AsBool(Get(extraParams, ODataConstants.CountParamName)),
            Expand: Get(extraParams, ODataConstants.ExpandParamName),
            RawAllSystem: new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(extraParams, StringComparer.InvariantCultureIgnoreCase)),
            Custom: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            Search: Get(extraParams, ODataConstants.SearchParamName),
            Compute: Get(extraParams, ODataConstants.ComputeParamName),
            Index: AsLong(Get(extraParams, ODataConstants.IndexParamName)),
            SkipToken: Get(extraParams, ODataConstants.SkipTokenParamName),
            DeltaToken: Get(extraParams, ODataConstants.DeltaTokenParamName)
        );
    }

    public SystemQueryOptions SystemQueryOptions { get; init; } = /* Empty */ new(
        Select: [],
        Filter: null,
        OrderBy: null,
        Top: null,
        Skip: null,
        Count: null,
        Expand: null,
        RawAllSystem: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
        Custom: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));


    public static Dictionary<string, string> ODataParams =
        new(StringComparer.InvariantCultureIgnoreCase)
        {
            [ODataConstants.SelectParamName] = $"[QueryString:{ODataConstants.SelectParamName}]",
            [ODataConstants.ExpandParamName] = $"[QueryString:{ODataConstants.ExpandParamName}]",
            [ODataConstants.FilterParamName] = $"[QueryString:{ODataConstants.FilterParamName}]",
            [ODataConstants.OrderByParamName] = $"[QueryString:{ODataConstants.OrderByParamName}]",
            [ODataConstants.TopParamName] = $"[QueryString:{ODataConstants.TopParamName}]",
            [ODataConstants.SkipParamName] = $"[QueryString:{ODataConstants.SkipParamName}]",
            [ODataConstants.CountParamName] = $"[QueryString:{ODataConstants.CountParamName}]",
            [ODataConstants.SearchParamName] = $"[QueryString:{ODataConstants.SearchParamName}]",
            [ODataConstants.ComputeParamName] = $"[QueryString:{ODataConstants.ComputeParamName}]",
            [ODataConstants.IndexParamName] = $"[QueryString:{ODataConstants.IndexParamName}]",
            [ODataConstants.SkipTokenParamName] = $"[QueryString:{ODataConstants.SkipTokenParamName}]",
            [ODataConstants.DeltaTokenParamName] = $"[QueryString:{ODataConstants.DeltaTokenParamName}]"
        };

    // Helpers (local parsing similar to SystemQueryOptionsParser)
    private static string? Get(IDictionary<string, string> dict, string key)
        => dict.TryGetValue(key, out var v) ? string.IsNullOrWhiteSpace(v) ? null : v : null;

    private static int? AsInt(string? s) => int.TryParse(s, out var i) ? i : null;

    private static long? AsLong(string? s) => long.TryParse(s, out var l) ? l : null;

    private static bool? AsBool(string? s)
    {
        if (s == null) return null;
        if (bool.TryParse(s, out var b)) return b;
        return s == "1" ? true : s == "0" ? false : null;
    }
}