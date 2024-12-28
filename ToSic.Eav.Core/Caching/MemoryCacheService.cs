using System.Runtime.Caching;
using ToSic.Lib.Services;

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

    public T Get<T>(string key, T fallback = default) => Cache.Get(key) is T typed ? typed : fallback;

    public bool TryGet<T>(string key, out T value)
    {
        value = default;
        if (!Cache.Contains(key))
            return false;

        var result = Cache.Get(key);

        // check type and null
        if (result is not T typed) return false;

        value = typed;
        return true;

    }

    public object Remove(string key) => Cache.Remove(key);

    public IPolicyMaker NewPolicyMaker() => new CacheItemPolicyMaker { Log = new Log(CacheItemPolicyMaker.LogName, Log) };

    /// <summary>
    /// WIP experimental - possible replacement with liquid API, to better see which methods are exactly being called.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="func"></param>
    public void Set(string key, object value, Func<IPolicyMaker, IPolicyMaker> func = default)
    {
        var l = Log.Fn($"key: '{key}'");
        try
        {
            var specs = NewPolicyMaker();
            var parsedSpecs = func?.Invoke(specs) ?? specs;
            var policy = parsedSpecs.CreateResult();
            Cache.Set(key, value, policy);
            l.Done();
        }
        catch (Exception ex)
        {
            l.Done(ex);
        }
    }

    /// <summary>
    /// WIP experimental - possible replacement with liquid API, to better see which methods are exactly being called.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="policyMaker"></param>
    public void Set(string key, object value, IPolicyMaker policyMaker)
    {
        var l = Log.Fn($"key: '{key}'");
        try
        {
            var policy = policyMaker.CreateResult();
            Cache.Set(key, value, policy);
            l.Done();
        }
        catch (Exception ex)
        {
            l.Done(ex);
        }
    }

    /// <summary>
    /// Used to create cache item dependency on other cache items
    /// with CacheEntryChangeMonitor in cache policy.
    /// </summary>
    /// <param name="keys">list of cache cacheKeys of existing cache items to depend on</param>
    /// <returns></returns>
    internal static CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys)
        => Cache.CreateCacheEntryChangeMonitor(keys);

    #endregion


    #region Experimental - communicate through cachekey

    // Idea is that any large objects which should communicate expiry would leave a key in the cache with a permanent expiry
    // This way if something changes - like the features service, it can notify the cache, and everything dependant will be invalidated

    private const string NotifyCachePrefix = "Eav-Notify-";

    public static void Notify(ICanBeCacheDependency obj)
        => Cache.Set(ExpandDependencyId(obj), new Timestamped<DateTime>(DateTime.Now, DateTime.Now.Ticks), ObjectCache.InfiniteAbsoluteExpiration);
    
    internal static CacheEntryChangeMonitor CreateCacheNotifyMonitor(IEnumerable<ICanBeCacheDependency> keys)
    {
        var prefixed = (keys ?? []).Where(x => x != null).Select(ExpandDependencyId);
        return Cache.CreateCacheEntryChangeMonitor(prefixed);
    }

    private static string ExpandDependencyId(ICanBeCacheDependency obj) =>
        $"{(obj.CacheIsNotifyOnly ? NotifyCachePrefix : "")}{obj.CacheDependencyId}";

    #endregion

}