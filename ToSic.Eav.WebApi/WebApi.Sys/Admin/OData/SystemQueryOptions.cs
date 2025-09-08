namespace ToSic.Eav.WebApi.Sys.Admin.OData;

public record SystemQueryOptions(
    IReadOnlyList<string> Select,
    string? Filter,
    string? OrderBy,
    int? Top,
    int? Skip,
    bool? Count,
    string? Expand,
    IReadOnlyDictionary<string, string> RawAllSystem,
    IReadOnlyDictionary<string, string> Custom
);