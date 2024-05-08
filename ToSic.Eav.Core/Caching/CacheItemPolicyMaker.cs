using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching.CachingMonitors;
using ToSic.Eav.Internal.Features;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;
using static ToSic.Eav.Caching.MemoryCacheService;

namespace ToSic.Eav.Caching;

/// <summary>
/// WIP fluid API cache specs.
/// It should ensure that all cache variants are possible, but that our code can easily spot which ones are used.
/// </summary>
/// <param name="key"></param>
/// <param name="value"></param>
/// <param name="parentLog"></param>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class CacheItemPolicyMaker(string key, object value, ILog parentLog): HelperBase(parentLog, "Eav.CacSpx")
{
    public string Key => key;
    public object Value => value;
    public CacheItemPolicy Policy { get; } = new();

    public CacheItemPolicyMaker SetAbsoluteExpiration(DateTimeOffset absoluteExpiration)
    {
        Log.A($"Set {nameof(Policy.AbsoluteExpiration)} = {absoluteExpiration}");
        Policy.AbsoluteExpiration = absoluteExpiration;
        return this;
    }

    public CacheItemPolicyMaker SetSlidingExpiration(TimeSpan slidingExpiration)
    {
        Log.A($"Set {nameof(Policy.SlidingExpiration)} = {slidingExpiration}");
        Policy.SlidingExpiration = slidingExpiration;
        return this;
    }

    public CacheItemPolicyMaker AddFiles(IList<string> filePaths)
    {
        if (filePaths is not { Count: > 0 }) return this;
        Log.A($"Add {filePaths.Count} {nameof(HostFileChangeMonitor)}s");
        Policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths));
        return this;
    }

    public CacheItemPolicyMaker AddFolders(IDictionary<string, bool> folderPaths)
    {
        if (folderPaths is not { Count: > 0 }) return this;
        Log.A($"Add {folderPaths.Count} {nameof(FolderChangeMonitor)}s");
        Policy.ChangeMonitors.Add(new FolderChangeMonitor(folderPaths));
        return this;
    }

    public CacheItemPolicyMaker AddCacheKeys(IEnumerable<string> cacheKeys)
    {
        if (cacheKeys == null) return this;
        var keysClone = new List<string>(cacheKeys);
        if (keysClone.Count <= 0) return this;
        Log.A($"Add {keysClone.Count} {nameof(CreateCacheEntryChangeMonitor)}s");
        Policy.ChangeMonitors.Add(CreateCacheEntryChangeMonitor(keysClone));
        return this;
    }

    public CacheItemPolicyMaker AddAppStates(List<IAppStateChanges> appStates)
    {
        if (appStates is not { Count: > 0 }) return this;
        Log.A($"Add {appStates.Count} {nameof(AppResetMonitor)}s to invalidate on App-data change");
        foreach (var appState in appStates)
            Policy.ChangeMonitors.Add(new AppResetMonitor(appState));
        return this;
    }

    public CacheItemPolicyMaker ConnectFeaturesService(IEavFeaturesService featuresService)
    {
        if (featuresService == null) return this;
        Log.A($"Add {nameof(FeaturesResetMonitor)} to invalidate on Feature change");
        Policy.ChangeMonitors.Add(new FeaturesResetMonitor(featuresService));

        return this;
    }

    public CacheItemPolicyMaker AddUpdateCallback(CacheEntryUpdateCallback updateCallback)
    {
        if (updateCallback == null) return this;
        Log.A("Add UpdateCallback");
        Policy.UpdateCallback = updateCallback;
        return this;
    }

}