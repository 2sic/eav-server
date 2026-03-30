using System.Collections.ObjectModel;
using ToSic.Sys.Utils;
using static System.StringComparer;

namespace ToSic.Sys.OData;

/// <summary>
/// Helper to retrieve all OData parameters from the query string, and parse them into a ODataOptions object.
/// </summary>
public class QueryODataParams
{
    public static Dictionary<string, ODataOptions> CreateMany(Func<IDictionary<string, string>, IDictionary<string, string>> parseFunc, string[] streamNames, string? selectedStream = default)
    {
        streamNames = streamNames.Any()
            ? streamNames
            : ["Default"];

        var result = streamNames.ToDictionary(n => n, n => Create(parseFunc, n), OrdinalIgnoreCase);

        // If the caller explicitly selected exactly one stream, bare OData options should target that stream.
        // This preserves the old Default-stream behavior for multi-stream requests, while letting
        // /QueryName/Books?$filter=... and ?stream=Books&$top=... behave intuitively.
        var fallbackStream = GetSingleSelectedStreamOrNull(streamNames, selectedStream);
        if (fallbackStream.IsEmpty())
            return result;

        var unprefixed = Create(parseFunc);
        if (unprefixed.IsEmpty())
            return result;

        var current = result[fallbackStream];
        var mergedRaw = current.AllRaw
            .ToDictionary(pair => pair.Key, pair => pair.Value, OrdinalIgnoreCase);

        var mergedAny = false;
        foreach (var pair in unprefixed.AllRaw)
        {
            // Stream-prefixed parameters such as Books$filter keep precedence over the bare fallback,
            // but other bare parameters should still fill in independently.
            var key = pair.Key;
            if (mergedRaw.ContainsKey(key))
                continue;

            mergedRaw[key] = pair.Value;
            mergedAny = true;
        }

        if (!mergedAny)
            return result;

        result[fallbackStream] = CreateFromRaw(mergedRaw);

        return result;
    }

    public static ODataOptions Create(Func<IDictionary<string, string>, IDictionary<string, string>> parseFunc, string? streamName = default) =>
        CreateInternal(parseFunc, streamName.EqualsInsensitive("Default") ? default : streamName);

    private static ODataOptions CreateInternal(Func<IDictionary<string, string>, IDictionary<string, string>> parseFunc, string? streamName = default)
    {
        if (parseFunc == null!)
            return new();

        // Get url parameters by passing tokens into the configuration, then parsing the result with the ODataParams as keys
        var odataDic = parseFunc(GetODataParams(streamName));

        // filter out keys with empty values
        odataDic = odataDic
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return CreateFromRaw(odataDic);
    }

    private static ODataOptions CreateFromRaw(IDictionary<string, string> odataDic)
    {
        // Construct the options from raw values so parsed requests and merged stream fallbacks
        // always populate the typed OData fields in exactly the same way.
        return new()
        {
            AllRaw = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(odataDic, OrdinalIgnoreCase)),
            Custom = new Dictionary<string, string>(OrdinalIgnoreCase),
            Select = SystemQueryOptionsParser.ParseSelect(Get(odataDic, ODataConstants.SelectParamName)),
            Filter = Get(odataDic, ODataConstants.FilterParamName),
            OrderBy = Get(odataDic, ODataConstants.OrderByParamName),
            Top = AsInt(Get(odataDic, ODataConstants.TopParamName)), // long in OData spec, but int should be enough for us
            Skip = AsInt(Get(odataDic, ODataConstants.SkipParamName)), // long in OData spec, but int should be enough for us
            Count = AsBool(Get(odataDic, ODataConstants.CountParamName)),
            Expand = Get(odataDic, ODataConstants.ExpandParamName),
            Search = Get(odataDic, ODataConstants.SearchParamName),
            Compute = Get(odataDic, ODataConstants.ComputeParamName),
            Index = AsLong(Get(odataDic, ODataConstants.IndexParamName)),
            SkipToken = Get(odataDic, ODataConstants.SkipTokenParamName),
            DeltaToken = Get(odataDic, ODataConstants.DeltaTokenParamName)
        };
    }

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

    private static string? GetSingleSelectedStreamOrNull(IReadOnlyList<string> streamNames, string? selectedStream)
    {
        if (streamNames.Count != 1 || selectedStream.IsEmpty())
            return null;

        var selectedStreams = selectedStream
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();

        if (selectedStreams.Length != 1)
            return null;

        return streamNames.Single().EqualsInsensitive(selectedStreams[0])
            ? streamNames.Single()
            : null;
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
