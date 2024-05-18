using System.Runtime.Caching;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching.CachingMonitors;
using ToSic.Eav.Internal.Features;
using ToSic.Lib.FunFact;
using static ToSic.Eav.Caching.MemoryCacheService;
using static System.Runtime.Caching.ObjectCache;

namespace ToSic.Eav.Caching;

/// <summary>
/// WIP fluid API cache specs.
/// It should ensure that all cache variants are possible, but that our code can easily spot which ones are used.
/// </summary>
/// <param name="parentLog"></param>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class CacheItemPolicyMaker(ILog parentLog, IEnumerable<Action<CacheItemPolicy>> actions = default) : FunFactActionsBase<CacheItemPolicy>(parentLog, actions, "Eav.CacSpx"), IPolicyMaker
{

    private IPolicyMaker Next(Action<CacheItemPolicy> addition)
        => new CacheItemPolicyMaker((Log as Log)?.Parent, CloneActions(addition));

    public override CacheItemPolicy CreateResult()
    {
        var policy = Apply(new());

        if (policy.AbsoluteExpiration != InfiniteAbsoluteExpiration || policy.SlidingExpiration != NoSlidingExpiration)
            return policy;
        Log.A("No expiration set - this might lead to memory leaks. Please set an expiration. Will auto-set.");
        policy.SlidingExpiration = DefaultSlidingExpiration;
        return policy;
    }

    public IPolicyMaker SetAbsoluteExpiration(DateTimeOffset absoluteExpiration)
    {
        Log.A($"Set {nameof(CacheItemPolicy.AbsoluteExpiration)} = {absoluteExpiration}");
        return Next(p => p.AbsoluteExpiration = absoluteExpiration);
    }

    public IPolicyMaker SetSlidingExpiration(TimeSpan slidingExpiration)
    {
        Log.A($"Set {nameof(CacheItemPolicy.SlidingExpiration)} = {slidingExpiration}");
        return Next(p => p.SlidingExpiration = slidingExpiration);
    }

    public IPolicyMaker AddFiles(IList<string> filePaths)
    {
        if (filePaths is not { Count: > 0 }) return this;
        Log.A($"Add {filePaths.Count} {nameof(HostFileChangeMonitor)}s");
        return Next(p => p.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths)));
    }

    public IPolicyMaker AddFolders(IDictionary<string, bool> folderPaths)
    {
        if (folderPaths is not { Count: > 0 }) return this;
        Log.A($"Add {folderPaths.Count} {nameof(FolderChangeMonitor)}s");
        return Next(p => p.ChangeMonitors.Add(new FolderChangeMonitor(folderPaths)));
    }

    public IPolicyMaker AddCacheKeys(IEnumerable<string> cacheKeys)
    {
        if (cacheKeys == null) return this;
        var keysClone = new List<string>(cacheKeys);
        if (keysClone.Count <= 0) return this;
        Log.A($"Add {keysClone.Count} {nameof(CreateCacheEntryChangeMonitor)}s");
        return Next(p => p.ChangeMonitors.Add(CreateCacheEntryChangeMonitor(keysClone)));
    }

    public IPolicyMaker AddAppStates(List<IAppStateChanges> appStates)
    {
        if (appStates is not { Count: > 0 }) return this;
        Log.A($"Add {appStates.Count} {nameof(AppResetMonitor)}s to invalidate on App-data change");
        return Next(p => appStates.ForEach(appState => p.ChangeMonitors.Add(new AppResetMonitor(appState))));
    }

    public IPolicyMaker ConnectFeaturesService(IEavFeaturesService featuresService)
    {
        if (featuresService == null) return this;
        Log.A($"Add {nameof(FeaturesResetMonitor)} to invalidate on Feature change");
        return Next(p => p.ChangeMonitors.Add(new FeaturesResetMonitor(featuresService)));
    }

    public IPolicyMaker AddUpdateCallback(CacheEntryUpdateCallback updateCallback)
    {
        if (updateCallback == null) return this;
        Log.A("Add UpdateCallback");
        return Next(p => p.UpdateCallback = updateCallback);
    }

}