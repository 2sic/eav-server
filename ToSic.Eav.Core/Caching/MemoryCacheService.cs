using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching.CachingMonitors;
using ToSic.Eav.Internal.Features;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;

namespace ToSic.Eav.Caching;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class MemoryCacheService() : ServiceBase("Eav.MemCacheSrv")
{
    private static readonly MemoryCache Cache = MemoryCache.Default;
    private static readonly TimeSpan DefaultSlidingExpiration = new(1, 0, 0);

    #region General

    public bool Contains(string key) => Cache.Contains(key);

    public object Get(string key) => Cache.Get(key);

    public object Remove(string key) => Cache.Remove(key);

    public void Set(string key, object value,
        DateTimeOffset? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        IList<string> filePaths = null,
        IDictionary<string, bool> folderPaths = null,
        IEnumerable<string> cacheKeys = null,
        List<IAppStateChanges> appStates = null,
        IEavFeaturesService featuresService = null,
        CacheEntryUpdateCallback updateCallback = null)
    {
        var policy = absoluteExpiration.HasValue
            ? new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration.Value }
            : new CacheItemPolicy { SlidingExpiration = slidingExpiration ?? DefaultSlidingExpiration };

        if (filePaths is { Count: > 0 })
            policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths));

        if (folderPaths is { Count: > 0 })
            policy.ChangeMonitors.Add(new FolderChangeMonitor(folderPaths));

        if (cacheKeys != null)
        {
            var keysClone = new List<string>(cacheKeys);
            if (keysClone.Count > 0)
                policy.ChangeMonitors.Add(CreateCacheEntryChangeMonitor(keysClone));
        }

        if (appStates?.Any() == true)
            foreach (var appState in appStates)
                policy.ChangeMonitors.Add(new AppResetMonitor(appState));

        // flush cache when any feature is changed
        // IMPORTANT: The featuresDoNotConnect service should NOT be connected to the log chain!
        if (featuresService != null)
            policy.ChangeMonitors.Add(new FeaturesResetMonitor(featuresService));

        if (updateCallback != null)
            policy.UpdateCallback = updateCallback;

        Cache.Set(new CacheItem(key, value), policy);
    }

    public bool Add(string key, object value, CacheItemPolicy policy) => Cache.Add(key, value, policy);

    /// <summary>
    /// Used to create cache item dependency on other cache items
    /// with CacheEntryChangeMonitor in cache policy.
    /// </summary>
    /// <param name="keys">list of cache cacheKeys of existing cache items to depend on</param>
    /// <returns></returns>
    public CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys) => Cache.CreateCacheEntryChangeMonitor(keys);

    #endregion
}