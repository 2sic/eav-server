using ToSic.Eav.Apps;
using ToSic.Eav.Apps.State;

namespace ToSic.Eav.DataSource.Caching.CacheInfo;

/// <summary>
/// Custom Cache Information which only check the app-state for time-stamp.
/// It also includes a suffix "more" to ensure than various items have unique cache-keys.
/// </summary>
internal class CacheInfoAppAndMore: ICacheInfo
{
    /// <summary>
    /// The prefix for the cache-key
    /// </summary>
    public readonly string Prefix;

    /// <summary>
    /// The App-State which is accessed to check time-stamps
    /// </summary>
    public readonly IAppStateCache App;

    /// <summary>
    /// A suffix to add to the cache key
    /// </summary>
    public readonly string More;

    public CacheInfoAppAndMore(string prefix, IAppStateCache app, string more)
    {
        Prefix = prefix;
        App = app;
        More = more;
    }

    public string CachePartialKey => Prefix;

    public string CacheFullKey => Prefix + More;

    public long CacheTimestamp => App.CacheTimestamp;

    public bool CacheChanged(long dependentTimeStamp) => App.CacheChanged(dependentTimeStamp);
}