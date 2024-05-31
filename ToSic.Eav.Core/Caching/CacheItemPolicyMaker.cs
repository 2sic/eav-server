using System.Runtime.Caching;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching.CachingMonitors;
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
public class CacheItemPolicyMaker(ILog parentLog, IEnumerable<(string, Action<CacheItemPolicy>)> actions = default) : FunFactActionsBase<CacheItemPolicy>(parentLog, actions, "Eav.CacSpx"), IPolicyMaker
{
    private IPolicyMaker Next(string name, Action<CacheItemPolicy> addition)
        => new CacheItemPolicyMaker(parentLog, CloneActions((name, addition)));

    public override CacheItemPolicy CreateResult()
    {
        var l = Log.Fn<CacheItemPolicy>();
        var policy = Apply(new());

        if (policy.AbsoluteExpiration != InfiniteAbsoluteExpiration || policy.SlidingExpiration != NoSlidingExpiration)
            return l.Return(policy, "expiry has been set");

        policy.SlidingExpiration = DefaultSlidingExpiration;
        return l.Return(policy, "No expiration set. Will auto-set.");
    }

    public IPolicyMaker SetAbsoluteExpiration(DateTimeOffset absoluteExpiration)
        => Next(
            $"Set {nameof(CacheItemPolicy.AbsoluteExpiration)} = {absoluteExpiration}",
            p => p.AbsoluteExpiration = absoluteExpiration
        );

    public IPolicyMaker SetSlidingExpiration(TimeSpan slidingExpiration)
        => Next(
            $"Set {nameof(CacheItemPolicy.SlidingExpiration)} = {slidingExpiration}",
            p => p.SlidingExpiration = slidingExpiration
        );

    public IPolicyMaker WatchFiles(IList<string> filePaths) =>
        filePaths is not { Count: > 0 }
            ? this
            : Next(
                $"Add {filePaths.Count} {nameof(HostFileChangeMonitor)}s",
                p => p.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths))
            );

    public IPolicyMaker WatchFolders(IDictionary<string, bool> folderPaths) =>
        folderPaths is not { Count: > 0 }
            ? this
            : Next(
                $"Watch {folderPaths.Count} {nameof(FolderChangeMonitor)}s: \n {string.Join("\n", folderPaths.Select(pair => $"Folders: '{pair.Key}'; Subfolder: {pair.Value}"))}",
                p => p.ChangeMonitors.Add(new FolderChangeMonitor(folderPaths))
            );

    public IPolicyMaker WatchCacheKeys(IEnumerable<string> cacheKeys)
    {
        if (cacheKeys == null) return this;
        var keysClone = new List<string>(cacheKeys);
        return keysClone.Count <= 0
            ? this
            : Next(
                $"Watch {keysClone.Count} {nameof(CreateCacheEntryChangeMonitor)}s",
                p => p.ChangeMonitors.Add(CreateCacheEntryChangeMonitor(keysClone))
            );
    }
    public IPolicyMaker WatchNotifyKeys(IEnumerable<string> cacheKeys)
    {
        if (cacheKeys == null) return this;
        var keysClone = new List<string>(cacheKeys);
        return keysClone.Count <= 0
            ? this
            : Next(
                $"Watch {keysClone.Count} {nameof(CreateCacheNotifyMonitor)}s",
                p => p.ChangeMonitors.Add(CreateCacheNotifyMonitor(keysClone))
            );
    }

    public IPolicyMaker WatchApps(List<IAppStateChanges> appStates) =>
        appStates is not { Count: > 0 }
            ? this
            : Next(
                $"Watch {appStates.Count} {nameof(AppResetMonitor)}s to invalidate on App-data change",
                p => appStates.ForEach(appState => p.ChangeMonitors.Add(new AppResetMonitor(appState)))
            );

    public IPolicyMaker WatchCallback(CacheEntryUpdateCallback updateCallback) =>
        updateCallback == null
            ? this
            : Next(
                "Add UpdateCallback",
                p => p.UpdateCallback = updateCallback
            );
}