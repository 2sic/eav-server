using System.Collections.ObjectModel;
using ToSic.Sys.Utils;
using static System.StringComparer;

namespace ToSic.Sys.OData;

/// <summary>
/// Helper to retrieve all OData parameters from the query string, and parse them into a SystemQueryOptions object.
/// </summary>
public class QueryODataParams
{
    public static Dictionary<string, SystemQueryOptions> CreateMany(Func<IDictionary<string, string>, IDictionary<string, string>> parseFunc, string[] streamNames)
    {
        streamNames = streamNames.Any()
            ? streamNames
            : ["Default"];
        return streamNames.ToDictionary(n => n, n => Create(parseFunc, n), OrdinalIgnoreCase);
    }

    public static SystemQueryOptions Create(Func<IDictionary<string, string>, IDictionary<string, string>> parseFunc, string? streamName = default) =>
        CreateInternal(parseFunc, streamName.EqualsInsensitive("Default") ? default : streamName);

    private static SystemQueryOptions CreateInternal(Func<IDictionary<string, string>, IDictionary<string, string>> parseFunc, string? streamName = default)
    {
        if (parseFunc == null!)
            return SystemQueryOptionsEmpty;

        // Get url parameters by passing tokens into the configuration, then parsing the result with the ODataParams as keys
        var odataDic = parseFunc(GetODataParams(streamName));

        // filter out keys with empty values
        odataDic = odataDic
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // Construct the options
        return new(
            RawAllSystem: new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(odataDic, OrdinalIgnoreCase)),
            Custom: new Dictionary<string, string>(OrdinalIgnoreCase),
            Select: SystemQueryOptionsParser.ParseSelect(Get(odataDic, ODataConstants.SelectParamName)),
            Filter: Get(odataDic, ODataConstants.FilterParamName),
            OrderBy: Get(odataDic, ODataConstants.OrderByParamName),
            Top: AsInt(Get(odataDic, ODataConstants.TopParamName)), // long in OData spec, but int should be enough for us
            Skip: AsInt(Get(odataDic, ODataConstants.SkipParamName)), // long in OData spec, but int should be enough for us
            Count: AsBool(Get(odataDic, ODataConstants.CountParamName)),
            Expand: Get(odataDic, ODataConstants.ExpandParamName),
            Search: Get(odataDic, ODataConstants.SearchParamName),
            Compute: Get(odataDic, ODataConstants.ComputeParamName),
            Index: AsLong(Get(odataDic, ODataConstants.IndexParamName)),
            SkipToken: Get(odataDic, ODataConstants.SkipTokenParamName),
            DeltaToken: Get(odataDic, ODataConstants.DeltaTokenParamName)
        );
    }

    private static readonly SystemQueryOptions SystemQueryOptionsEmpty = /* Empty */ new(
        Select: [],
        Filter: null,
        OrderBy: null,
        Top: null,
        Skip: null,
        Count: null,
        Expand: null,
        RawAllSystem: new Dictionary<string, string>(OrdinalIgnoreCase),
        Custom: new Dictionary<string, string>(OrdinalIgnoreCase));


    /// <summary>
    /// Experimental: try to get OData parameters for multiple streams, using a prefix such as "Authors$select" for the "Authors" stream.
    /// This is not part of the OData spec, but could be useful in some scenarios.
    /// </summary>
    /// <param name="streamName"></param>
    /// <returns></returns>
    private static Dictionary<string, string> GetODataParams(string? streamName = default)
    {
        if (streamName.IsEmpty())
            return ODataParams;

        return ODataParams
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Replace(":", $":{streamName}")
            );
    }

    internal static readonly Dictionary<string, string> ODataParams =
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