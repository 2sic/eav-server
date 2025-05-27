using System.Runtime.Caching;
using ToSic.Lib.Caching.Monitors;
using ToSic.Lib.FunFact;
using static ToSic.Lib.Caching.MemoryCacheService;
using static System.Runtime.Caching.ObjectCache;

namespace ToSic.Lib.Caching.Policies;

/// <summary>
/// WIP fluid API cache specs.
/// It should ensure that all cache variants are possible, but that our code can easily spot which ones are used.
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record CacheItemPolicyMaker : FunFactActionsBase<CacheItemPolicy>, IPolicyMaker
{
    internal static string LogName = "Eav.CacSpx";

    private IPolicyMaker Next(string name, Action<CacheItemPolicy> addition)
        => this with { Actions = CloneActions((name, addition)) };

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

    public IPolicyMaker SetSlidingExpiration(int seconds)
        => SetSlidingExpiration(new TimeSpan(0, 0, seconds));

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

    public IPolicyMaker WatchNotifyKeys(IEnumerable<ICanBeCacheDependency> cacheKeys)
    {
        if (cacheKeys == null) return this;
        var keysClone = new List<ICanBeCacheDependency>(cacheKeys);
        return !keysClone.Any()
            ? this
            : Next(
                $"Watch {keysClone.Count} {nameof(CreateCacheNotifyMonitor)}s",
                p => p.ChangeMonitors.Add(CreateCacheNotifyMonitor(keysClone))
            );
    }

    public IPolicyMaker WatchCallback(CacheEntryUpdateCallback updateCallback) =>
        updateCallback == null
            ? this
            : Next(
                "Add UpdateCallback",
                p => p.UpdateCallback = updateCallback
            );
}