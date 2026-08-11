namespace ToSic.Sys.Memory;

/// <summary>
/// Marks objects which can self-report their size in memory.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ICanEstimateSize
{
    /// <summary>
    /// Method to implement when an object can self-report it's size.
    /// </summary>
    /// <param name="log">An optional logger to use for logging during the size estimation.</param>
    /// <returns>The estimated size of the object.</returns>
    public SizeEstimate EstimateSize(ILog? log = default);
}