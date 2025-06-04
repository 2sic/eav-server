using ToSic.Eav.Apps.Sys;

namespace ToSic.Eav.DataSource.Internal.Caching;

/// <summary>
/// Custom Cache Information which only check the app-state for time-stamp.
/// It also includes a suffix "more" to ensure than various items have unique cache-keys.
/// </summary>
internal class CacheInfoAppAndMore(string prefix, IAppStateCache app, string more) : ICacheInfo
{
    /// <summary>
    /// The prefix for the cache-key
    /// </summary>
    public readonly string Prefix = prefix;

    /// <summary>
    /// The App-State which is accessed to check time-stamps
    /// </summary>
    public readonly IAppStateCache App = app;

    /// <summary>
    /// A suffix to add to the cache key
    /// </summary>
    public readonly string More = more;

    public string CachePartialKey => Prefix;

    public string CacheFullKey => Prefix + More;

    public long CacheTimestamp => App.CacheTimestamp;

    public bool CacheChanged(long dependentTimeStamp) => App.CacheChanged(dependentTimeStamp);
}