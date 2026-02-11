namespace ToSic.Sys.OData;

public record SystemQueryOptions
{

    public IReadOnlyDictionary<string, string> RawAllSystem
    {
        get => field ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        init;
    }

    public IReadOnlyDictionary<string, string> Custom
    {
        get => field ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        init;
    }

    // Implemented
    public IReadOnlyList<string> Select { get => field ??= []; init; }
    public string? Filter { get; init; }
    public string? OrderBy { get; init; }
    public int? Top { get; init; }
    public int? Skip { get; init; }
    public bool? Count { get; init; }
    public string? Expand { get; init; }

    // TODO: $search, $compute, $index, $skiptoken, $deltatoken
    public string? Search { get; init; }
    public string? Compute { get; init; }
    public long? Index { get; init; }
    public string? SkipToken { get; init; }
    public string? DeltaToken { get; init; }


    public bool IsEmpty() => !RawAllSystem.Any();

    public bool IsEmptyExceptForSelect() =>
        IsEmpty()
        || (RawAllSystem.Count == 1 && RawAllSystem.ContainsKey(ODataConstants.SelectParamName));

}