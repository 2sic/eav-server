namespace ToSic.Sys.OData;

public record SystemQueryOptions(
    IReadOnlyList<string> Select,
    string? Filter,
    string? OrderBy,
    int? Top, // long in OData spec, but int should be enough for us
    int? Skip, // long in OData spec, but int should be enough for us
    bool? Count,
    string? Expand,
    IReadOnlyDictionary<string, string> RawAllSystem,
    IReadOnlyDictionary<string, string> Custom,

    // TODO: $search, $compute, $index, $skiptoken, $deltatoken
    string? Search = null,
    string? Compute = null,
    long? Index = null,
    string? SkipToken = null,
    string? DeltaToken = null
);