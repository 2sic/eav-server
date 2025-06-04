namespace ToSic.Sys.Caching;

/// <summary>
/// Marks something that is cache-dependent. Used for things that are themselves cached, but rely on an upstream cache. 
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ICacheDependent: ITimestamped
{
    /// <summary>
    /// Find out if the source it depends on has changed.
    /// </summary>
    /// <returns>True if the upstream cache returns a newer timestamp</returns>
    bool CacheChanged();
}