using System.Runtime.Caching;
using ToSic.Eav.Caching;

namespace ToSic.Lib.Caching.Policies;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IPolicyMaker
{
    CacheItemPolicy CreateResult();
    IPolicyMaker SetAbsoluteExpiration(DateTimeOffset absoluteExpiration);
    IPolicyMaker SetSlidingExpiration(TimeSpan slidingExpiration);

    /// <summary>
    /// Set sliding expiration in seconds.
    /// Numbers bigger than 60 are fine, as the number will be converted to ticks.
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns>A new PolicyMaker with the updated expiration</returns>
    IPolicyMaker SetSlidingExpiration(int seconds);

    IPolicyMaker WatchFiles(IList<string> filePaths);
    IPolicyMaker WatchFolders(IDictionary<string, bool> folderPaths);
    IPolicyMaker WatchCacheKeys(IEnumerable<string> cacheKeys);

    IPolicyMaker WatchNotifyKeys(IEnumerable<ICanBeCacheDependency> cacheKeys);
    IPolicyMaker WatchCallback(CacheEntryUpdateCallback updateCallback);
}