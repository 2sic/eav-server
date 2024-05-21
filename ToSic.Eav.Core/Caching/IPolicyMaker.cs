using System.Runtime.Caching;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Internal.Features;

namespace ToSic.Eav.Caching;

public interface IPolicyMaker
{
    CacheItemPolicy CreateResult();
    IPolicyMaker SetAbsoluteExpiration(DateTimeOffset absoluteExpiration);
    IPolicyMaker SetSlidingExpiration(TimeSpan slidingExpiration);
    IPolicyMaker WatchFiles(IList<string> filePaths);
    IPolicyMaker WatchFolders(IDictionary<string, bool> folderPaths);
    IPolicyMaker WatchCacheKeys(IEnumerable<string> cacheKeys);
    IPolicyMaker WatchApps(List<IAppStateChanges> appStates);
    IPolicyMaker WatchFeaturesService(IEavFeaturesService featuresService);
    IPolicyMaker WatchCallback(CacheEntryUpdateCallback updateCallback);
}