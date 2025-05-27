namespace ToSic.Lib.Caching.Keys;

/// <summary>
/// Marks objects which can identify what cache it's for. <br/>
/// For example, when parameters change what data is cached, then the cache-key can contain this parameter,
/// so that a different cache is used based on changing parameters. 
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ICacheKey
{
    /// <summary>
    /// Unique key-id for this specific situation - could be the same for all instances, or could vary by some parameter.
    /// </summary>
    /// <returns>A string which is specific to this cache-item.</returns>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    string CachePartialKey { get; }

    /// <summary>
    /// Combination of the current key and all keys of upstream cached items, to create a long unique key for this context.
    /// </summary>
    /// <returns>Full key containing own partial key and upstream keys.</returns>
    [ShowApiWhenReleased(ShowApiMode.Never)] 
    string CacheFullKey { get; }
}