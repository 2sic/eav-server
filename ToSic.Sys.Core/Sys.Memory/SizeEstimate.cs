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
    /// <summary>
    /// Total size, combining known and estimated sizes.
    /// </summary>
    public int Total => Known + Estimated;

    /// <summary>
    /// Size indication icon, which can be a warning, unknown, or check mark depending on the precision.
    /// </summary>
    public string Icon => IsError
        ? "⚠️"
        : IsUnknown || Known == 0
            ? "❔"
            : "✅";

    /// <summary>
    /// Add two size estimates together. The known and estimated sizes are added, and the unknown and error flags are combined with a logical OR.
    /// </summary>
    /// <param name="a">The first size estimate.</param>
    /// <param name="b">The second size estimate.</param>
    /// <returns>The combined size estimate.</returns>
    public static SizeEstimate operator +(SizeEstimate a, SizeEstimate b) => new(
        Known: a.Known + b.Known,
        Estimated: a.Estimated + b.Estimated,
        Expanded: a.Expanded + b.Expanded,
        IsUnknown: a.IsUnknown || b.IsUnknown,
        IsError: a.IsError || b.IsError
    );

    /// <summary>
    /// Subtract one size estimate from another. The known and estimated sizes are subtracted, and the unknown and error flags are combined with a logical OR.
    /// </summary>
    /// <param name="a">The first size estimate.</param>
    /// <param name="b">The second size estimate.</param>
    /// <returns>The resulting size estimate after subtraction.</returns>
    public static SizeEstimate operator -(SizeEstimate a, SizeEstimate b) => new(
        Known: a.Known - b.Known,
        Estimated: a.Estimated - b.Estimated,
        Expanded: a.Expanded - b.Expanded,
        IsUnknown: a.IsUnknown || b.IsUnknown,
        IsError: a.IsError || b.IsError
    );
}