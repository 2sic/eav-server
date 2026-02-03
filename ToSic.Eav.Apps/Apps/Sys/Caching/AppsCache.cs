using System.Collections.Concurrent;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Sys.Caching;

/// <summary>
/// The default Apps Cache system running on a normal environment.
/// </summary>
[PrivateApi]
internal class AppsCache(IRuntimeKeyService runtimeKeyService)
    : AppsCacheBase(runtimeKeyService), IAppsCacheSwitchable
{
    #region SwitchableService

    public const string DefaultNameId = "DefaultCache";

    public override string NameId => DefaultNameId;

    public override bool IsViable() => true;

    public override int Priority => 1;

    #endregion

    public override IReadOnlyDictionary<int, Zone> Zones(IAppLoaderTools tools)
    {
        // Cache zones per-tenant runtime key; if unavailable, skip caching to avoid cross-tenant bleed.
        if (!TryZoneCacheKey(out var cacheKey))
            return LoadZones(tools);

        var lazy = ZoneAppCaches.GetOrAdd(cacheKey,
            // Lazy ensures only one LoadZones per tenant key, even under concurrency.
            _ => new Lazy<IReadOnlyDictionary<int, Zone>>(() => LoadZones(tools)));
        return lazy.Value;
    }

    // Returns true only when we have a tenant-aware runtime key to safely partition zone caches.
    private bool TryZoneCacheKey(out string cacheKey)
    {
        try
        {
            // Use a stable app identity with no DB lookup; tenant-aware runtimes will encode tenant here.
            var identity = new AppIdentity(KnownAppsConstants.DefaultZoneId, KnownAppsConstants.AppIdEmpty);
            // Use runtime key presence as the guard for safe, tenant-scoped caching.
            cacheKey = runtimeKeyService.AppRuntimeKey(identity);
            return cacheKey.HasValue();
        }
        catch
        {
            // Any failure means we cannot reliably scope cache entries to a tenant.
            cacheKey = string.Empty;
            return false;
        }
    }

    // Per-tenant zone cache keyed by runtime key.
    private static readonly ConcurrentDictionary<string, Lazy<IReadOnlyDictionary<int, Zone>>> ZoneAppCaches = new();


    #region The cache-variable + HasCacheItem, SetCacheItem, Get, Remove

    // Global app-state cache shared by all instances; entries are runtime-key scoped.
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
            throw new("issue with setting cache item - key is '" + key + "' and cache is null =" +
                      (Caches == null!) + " and item is null=" + (item == null!), ex);
        }
    }

    /// <inheritdoc />
    protected override IAppStateCache Get(string key) => Caches[key];

    /// <inheritdoc />
    protected override void Remove(string key) => Caches.Remove(key);    // returns false if key was not found (no Exception)

    #endregion

    /// <inheritdoc />
    public override void PurgeZones() => ZoneAppCaches.Clear();

}
