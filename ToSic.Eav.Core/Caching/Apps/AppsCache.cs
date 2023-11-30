using System;
using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Caching;

/// <summary>
/// The default Apps Cache system running on a normal environment. 
/// </summary>
[PrivateApi]
internal class AppsCache: AppsCacheBase, IAppsCacheSwitchable
{
    #region SwitchableService

    public const string DefaultNameId = "DefaultCache";

    public override string NameId => DefaultNameId;

    public override bool IsViable() => true;

    public override int Priority => 1;

    #endregion

    public override IReadOnlyDictionary<int, Zone> Zones(IAppLoaderTools tools)
    {
        // ensure it's only loaded once, even if multiple threads are trying this at the same time
        if (ZoneAppCache != null) return ZoneAppCache;
        lock (ZoneAppLoadLock)
            if (ZoneAppCache == null)
                ZoneAppCache = LoadZones(tools);
        return ZoneAppCache;
    }


    // note: this object must be volatile!
    [PrivateApi] protected static volatile IReadOnlyDictionary<int, Zone> ZoneAppCache;
    [PrivateApi] protected static readonly object ZoneAppLoadLock = new();


    #region The cache-variable + HasCacheItem, SetCacheItem, Get, Remove

    private static readonly IDictionary<string, IAppStateCache> Caches = new Dictionary<string, IAppStateCache>();

    /// <inheritdoc />
    protected override bool Has(string cacheKey) => Caches.ContainsKey(cacheKey);

    /// <inheritdoc />
    protected override void Set(string key, IAppStateCache item)
    {
        try
        {
            // add or create
            // 2018-03-28 added lock - because I assume that's the cause of the random errors sometimes on system-load - see #1498
            lock (Caches)
            {
                Caches[key] = item;
            }
        }
        catch (Exception ex)
        {
            // unclear why this pops up sometime...if it would also hit on live, so I'm adding some more info
            throw new Exception("issue with setting cache item - key is '" + key + "' and cache is null =" +
                                (Caches == null) + " and item is null=" + (item == null), ex);
        }
    }

    /// <inheritdoc />
    protected override IAppStateCache Get(string key) => Caches[key];

    /// <inheritdoc />
    protected override void Remove(string key) => Caches.Remove(key);    // returns false if key was not found (no Exception)

    #endregion

    /// <inheritdoc />
    public override void PurgeZones() => ZoneAppCache = null;

}