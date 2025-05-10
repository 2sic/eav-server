namespace ToSic.Eav.Caching;

/// <summary>
/// Marks objects which are cache-based, and which may contain obsolete cached data.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ICacheExpiring: ITimestamped
{
    /// <summary>
    /// Detect if the cache has newer data.
    /// It's called using the TimeStamp of the dependent object which may still have old data.
    /// </summary>
    /// <param name="dependentTimeStamp">New time stamp of a dependent object, which could have an older timestamp.</param>
    /// <returns>True if the timestamps differ, false if it's the same</returns>
    /// <remarks>
    /// This is implemented in each object, because sometimes it compares its own timestamp, sometimes that of another underlying object.
    /// </remarks>
    bool CacheChanged(long dependentTimeStamp);

}