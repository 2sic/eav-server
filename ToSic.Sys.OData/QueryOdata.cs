using System.Collections.ObjectModel;
using static System.StringComparer;

namespace ToSic.Sys.OData;

/// <summary>
/// Helper to retrieve all OData parameters from the query string, and parse them into a SystemQueryOptions object.
/// </summary>
public class QueryODataParams
{
    public QueryODataParams(Func<IDictionary<string, string>, IDictionary<string, string>> parseFunc)
    {
        if (parseFunc == null)
            return;

        // Get url parameters by passing tokens into the configuration, then parsing the result with the ODataParams as keys
        var extraParams = parseFunc(ODataParams);

        // filter out keys with empty values
        extraParams = extraParams
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // Construct the options
        SystemQueryOptions = new(
            RawAllSystem: new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(extraParams, InvariantCultureIgnoreCase)),
            Custom: new Dictionary<string, string>(OrdinalIgnoreCase),
            Select: SystemQueryOptionsParser.ParseSelect(Get(extraParams, ODataConstants.SelectParamName)),
            Filter: Get(extraParams, ODataConstants.FilterParamName),
            OrderBy: Get(extraParams, ODataConstants.OrderByParamName),
            Top: AsInt(Get(extraParams, ODataConstants.TopParamName)), // long in OData spec, but int should be enough for us
            Skip: AsInt(Get(extraParams, ODataConstants.SkipParamName)), // long in OData spec, but int should be enough for us
            Count: AsBool(Get(extraParams, ODataConstants.CountParamName)),
            Expand: Get(extraParams, ODataConstants.ExpandParamName),
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
        RawAllSystem: new Dictionary<string, string>(OrdinalIgnoreCase),
        Custom: new Dictionary<string, string>(OrdinalIgnoreCase));


    public static Dictionary<string, string> ODataParams =
        new(InvariantCultureIgnoreCase)
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
        if (s == null)
            return null;
        if (bool.TryParse(s, out var b))
            return b;
        return s switch
        {
            "1" => true,
            "0" => false,
            _ => null
        };
    }
}