namespace ToSic.Sys.Caching.Keys;

/// <summary>
/// Generates cache-keys for certain objects and combines them with CacheKeys of parent-objects which this object relies on.  
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ICacheKeyManager: ICacheKey
{
    string[] SubKeys { get; }
}