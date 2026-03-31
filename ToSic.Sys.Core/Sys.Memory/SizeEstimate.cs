namespace ToSic.Sys.Memory;

/// <summary>
/// Estimate the size of something - usually for cache-size estimates.
/// </summary>
/// <param name="Known">The known amount of data.</param>
/// <param name="Estimated">Any estimate, which will either replace the known amount or be added to it.</param>
/// <param name="IsUnknown">Indicates if the size is unknown.</param>
/// <param name="IsError">Indicates if there was an error estimating the size.</param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record SizeEstimate(
    int Known = 0,
    int Estimated = 0,
    int Expanded = 0,
    bool IsUnknown = false,
    bool IsError = false
)
{
    public int Total => Known + Estimated;

    public string Icon => IsError
        ? "⚠️"
        : IsUnknown || Known == 0
            ? "❔"
            : "✅";

    public static SizeEstimate operator +(SizeEstimate a, SizeEstimate b) => new(
        Known: a.Known + b.Known,
        Estimated: a.Estimated + b.Estimated,
        Expanded: a.Expanded + b.Expanded,
        IsUnknown: a.IsUnknown || b.IsUnknown,
        IsError: a.IsError || b.IsError
    );

    public static SizeEstimate operator -(SizeEstimate a, SizeEstimate b) => new(
        Known: a.Known - b.Known,
        Estimated: a.Estimated - b.Estimated,
        Expanded: a.Expanded - b.Expanded,
        IsUnknown: a.IsUnknown || b.IsUnknown,
        IsError: a.IsError || b.IsError
    );
}