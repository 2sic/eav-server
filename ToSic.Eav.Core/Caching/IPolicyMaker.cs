using System.Runtime.Caching;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Internal.Features;

namespace ToSic.Eav.Caching;

public interface IPolicyMaker
{
    CacheItemPolicy CreateResult();
    IPolicyMaker SetAbsoluteExpiration(DateTimeOffset absoluteExpiration);
    IPolicyMaker SetSlidingExpiration(TimeSpan slidingExpiration);
    IPolicyMaker AddFiles(IList<string> filePaths);
    IPolicyMaker AddFolders(IDictionary<string, bool> folderPaths);
    IPolicyMaker AddCacheKeys(IEnumerable<string> cacheKeys);
    IPolicyMaker AddAppStates(List<IAppStateChanges> appStates);
    IPolicyMaker ConnectFeaturesService(IEavFeaturesService featuresService);
    IPolicyMaker AddUpdateCallback(CacheEntryUpdateCallback updateCallback);
}