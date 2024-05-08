using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching.CachingMonitors;
using ToSic.Eav.Internal.Features;
using ToSic.Lib.Coding;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;
using static System.Runtime.Caching.ObjectCache;

namespace ToSic.Eav.Caching;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class MemoryCacheService() : ServiceBase("Eav.MemCacheSrv")
{
    private static readonly MemoryCache Cache = MemoryCache.Default;
    internal static readonly TimeSpan DefaultSlidingExpiration = new(1, 0, 0);

    #region General

    public bool Contains(string key) => Cache.Contains(key);

    public object Get(string key) => Cache.Get(key);

    public object Remove(string key) => Cache.Remove(key);

    /// <summary>
    /// WIP experimental - possible replacement with liquid API, to better see which methods are exactly being called.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="func"></param>
    public void SetNew(string key, object value, Func<CacheItemPolicyMaker, CacheItemPolicyMaker> func)
    {
        var l = Log.Fn($"key: '{key}'");
        try
        {
            var specs = new CacheItemPolicyMaker(key, value, Log);
            specs = func(specs);
            var policy = specs.Policy;
            if (policy.AbsoluteExpiration == InfiniteAbsoluteExpiration && policy.SlidingExpiration == NoSlidingExpiration)
            {
                l.A("No expiration set - this might lead to memory leaks. Please set an expiration. Will auto-set.");
                policy.SlidingExpiration = DefaultSlidingExpiration;
            }
            Cache.Set(new(specs.Key, specs.Value), specs.Policy);
            l.Done();
        }
        catch (Exception ex)
        {
            l.Done(ex);
        }
    }

    public void Set(string key, object value,
        NoParamOrder protector = default,
        DateTimeOffset? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        IList<string> filePaths = null,
        IDictionary<string, bool> folderPaths = null,
        IEnumerable<string> cacheKeys = null,
        List<IAppStateChanges> appStates = null,
        IEavFeaturesService featuresService = null,
        CacheEntryUpdateCallback updateCallback = null)
    {
        var l = Log.Fn($"key: '{key}'");
        try
        {
            CacheItemPolicy policy = absoluteExpiration.HasValue
                ? new() { AbsoluteExpiration = absoluteExpiration.Value }
                : new() { SlidingExpiration = slidingExpiration ?? DefaultSlidingExpiration };

            if (filePaths is { Count: > 0 })
            {
                l.A($"Add {filePaths.Count} {nameof(HostFileChangeMonitor)}s");
                policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths));
            }

            if (folderPaths is { Count: > 0 })
            {
                l.A($"Add {folderPaths.Count} {nameof(FolderChangeMonitor)}s");
                policy.ChangeMonitors.Add(new FolderChangeMonitor(folderPaths));
            }

            if (cacheKeys != null)
            {
                var keysClone = new List<string>(cacheKeys);
                if (keysClone.Count > 0)
                {
                    l.A($"Add {keysClone.Count} Cache-Entry Change Monitors");
                    policy.ChangeMonitors.Add(CreateCacheEntryChangeMonitor(keysClone));
                }
            }

            if (appStates?.Any() == true)
            {
                l.A($"Add {appStates.Count} {nameof(AppResetMonitor)}s to invalidate on App-data change");
                foreach (var appState in appStates)
                    policy.ChangeMonitors.Add(new AppResetMonitor(appState));
            }

            // flush cache when any feature is changed
            // IMPORTANT: The featuresDoNotConnect service should NOT be connected to the log chain!
            if (featuresService != null)
            {
                l.A($"Add {nameof(FeaturesResetMonitor)} to invalidate on Feature change");
                policy.ChangeMonitors.Add(new FeaturesResetMonitor(featuresService));
            }

            if (updateCallback != null)
            {
                l.A("Add UpdateCallback");
                policy.UpdateCallback = updateCallback;
            }

            Cache.Set(new(key, value), policy);
            l.Done();
        }
        catch (Exception ex)
        {
            l.Done(ex);
        }
    }

    public bool Add(string key, object value, CacheItemPolicy policy) => Cache.Add(key, value, policy);

    /// <summary>
    /// Used to create cache item dependency on other cache items
    /// with CacheEntryChangeMonitor in cache policy.
    /// </summary>
    /// <param name="keys">list of cache cacheKeys of existing cache items to depend on</param>
    /// <returns></returns>
    internal static CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys) => Cache.CreateCacheEntryChangeMonitor(keys);

    #endregion

}