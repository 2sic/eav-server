namespace ToSic.Sys.OData;

public record ODataOptions
{
    /// <summary>
    /// Contains all settings in raw form, mainly for detecting if any settings were applied.
    /// </summary>
    public IReadOnlyDictionary<string, string> AllRaw
    {
        get => field ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        init;
    }

    /// <summary>
    /// Contains all raw parameters which don't start with "$", for potential custom processing.
    /// This is not part of the OData spec, but could be useful in some scenarios.
    /// As of 2026-02, I believe it's not really used... could be a leftover of initial development
    /// </summary>
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


    public bool IsEmpty() => !AllRaw.Any();

    public bool IsEmptyExceptForSelect() =>
        IsEmpty()
        || (AllRaw.Count == 1 && AllRaw.ContainsKey(ODataConstants.SelectParamName));

}