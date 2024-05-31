using System.Runtime.Caching;
using ToSic.Eav.Apps.State;

namespace ToSic.Eav.Caching;

public interface IPolicyMaker
{
    CacheItemPolicy CreateResult();
    IPolicyMaker SetAbsoluteExpiration(DateTimeOffset absoluteExpiration);
    IPolicyMaker SetSlidingExpiration(TimeSpan slidingExpiration);
    IPolicyMaker WatchFiles(IList<string> filePaths);
    IPolicyMaker WatchFolders(IDictionary<string, bool> folderPaths);
    IPolicyMaker WatchCacheKeys(IEnumerable<string> cacheKeys);

    IPolicyMaker WatchNotifyKeys(IEnumerable<ICanBeCacheDependency> cacheKeys);
    IPolicyMaker WatchCallback(CacheEntryUpdateCallback updateCallback);
}