using System.Collections.Generic;
using System.Runtime.Caching;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;

namespace ToSic.Eav.Caching;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class MemoryCacheService() : ServiceBase("Eav.MemCacheSrv")
{
    private static readonly MemoryCache Cache = MemoryCache.Default;


    #region General

    public bool Contains(string key) => Cache.Contains(key);

    public object Get(string key) => Cache.Get(key);

    public object Remove(string key) => Cache.Remove(key);

    public void Set(string key, object value, CacheItemPolicy policy) => Cache.Set(key, value, policy);

    public void Set(CacheItem item, CacheItemPolicy policy) => Cache.Set(item, policy);

    public bool Add(string key, object value, CacheItemPolicy policy) => Cache.Add(key, value, policy);

    /// <summary>
    /// Used to create cache item dependency on other cache items
    /// with CacheEntryChangeMonitor in cache policy.
    /// </summary>
    /// <param name="keys">list of cache keys of existing cache items to depend on</param>
    /// <returns></returns>
    public CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys) => Cache.CreateCacheEntryChangeMonitor(keys);

    #endregion
}