namespace ToSic.Sys.OData;

public record SystemQueryOptions(
    IReadOnlyDictionary<string, string> RawAllSystem,
    IReadOnlyDictionary<string, string> Custom,

    // Implemented
    IReadOnlyList<string> Select,
    string? Filter = null,
    string? OrderBy = null,
    int? Top = null, // long in OData spec, but int should be enough for us
    int? Skip = null, // long in OData spec, but int should be enough for us
    bool? Count = null,
    string? Expand = null,

    // TODO: $search, $compute, $index, $skiptoken, $deltatoken
    string? Search = null,
    string? Compute = null,
    long? Index = null,
    string? SkipToken = null,
    string? DeltaToken = null
)
{
    public bool IsEmpty() => !RawAllSystem.Any();

    public bool IsEmptyExceptForSelect() =>
        IsEmpty()
        || (RawAllSystem.Count == 1 && RawAllSystem.ContainsKey(ODataConstants.SelectParamName));
}