using System.Collections.Concurrent;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Persistence.Sys.AppState;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Sys.Caching;

/// <summary>
/// The local and default Apps Cache system running on a normal environment.
/// </summary>
/// <remarks>
/// To replace this, you can implement your own IAppsCache and register it in the DI system.
///
/// 1. Either create a full replacement implementing <see cref="IAppsCache"/> and register it both as <see cref="IAppsCache"/> and as the key it's using.
/// 2. Or create a lightweight wrapper and inject this as <see cref="IAppsCache"/> (it is also registered in DI).
/// </remarks>
[PrivateApi]
internal class AppsCacheInMemory(IAppCacheKeyService appCacheKeyService)
    : IAppsCache
{
    #region Keyed Service Identity

    public const string DefaultNameId = "DefaultCache";

    /// <summary>
    /// Identifies this cache to check if the one currently used matches this one.
    /// </summary>
    public string NameId => DefaultNameId;

    #endregion

    public IReadOnlyDictionary<int, Zone> Zones(IZoneLoader tools)
    {
        // Cache zones per-tenant runtime key; if unavailable, skip caching to avoid cross-tenant bleed.
        if (!TryZoneCacheKey(out var cacheKey))
            return new ZoneCachePresetHelper().AddPresetZone(tools.Zones());

        // Lazy ensures only one LoadZones per tenant key, even under concurrency.
        var lazy = ZoneAppCaches.GetOrAdd(cacheKey, _ => new(() => new ZoneCachePresetHelper().AddPresetZone(tools.Zones())));
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
            cacheKey = appCacheKeyService.AppCacheKey(identity);
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
    private static readonly ConcurrentDictionary<string, IAppStateCache> Caches = new();

    /// <inheritdoc />
    public bool Has(IAppIdentity app)
        => Caches.ContainsKey(appCacheKeyService.AppCacheKey(app));

    [PrivateApi]
    public void Add(IAppStateCache appState)
        => Caches.Set(appCacheKeyService.AppCacheKey(appState), appState);


    /// <inheritdoc />
    public IAppStateCache Get(IAppIdentity app, IAppLoaderTools tools)
        => Caches.GetOrBuild(appCacheKeyService, tools, app);


    /// <inheritdoc />
    public void Load(IAppIdentity app, string primaryLanguage, IAppLoaderTools tools)
        => Caches.GetOrBuild(appCacheKeyService, tools, app, primaryLanguage);

    #endregion


    #region Update

    /// <inheritdoc />
    public void Update(IAppIdentity appIdentity, IEnumerable<int> entities, ILog log, IAppLoaderTools tools)
    {
        var entityIds = entities.ToArray();
        var l = log.Fn($"{appIdentity}, {entityIds.Length} entities");
        // if it's not cached yet, ignore the request as partial update won't be necessary
        if (!Has(appIdentity))
            l.Done("not cached, won't update; probably updating an app in the process of being created.");
        //throw new NullReferenceException($"Trying to update an app which doesn't exist in the cache. This should never happen."); 
        var appState = Get(appIdentity, tools);
        if (appState == null)
            throw new NullReferenceException($"AppState was returned null. This should never happen.");
        tools.RepositoryLoader(log).Update(appState, AppStateLoadSequence.ItemLoad, new(), [.. entityIds]);
        l.Done("ok");
    }

    #endregion


    #region Purge Cache

    /// <inheritdoc />
    public void Purge(IAppIdentity app)
    {
        var key = appCacheKeyService.AppCacheKey(app);
        if (Caches.TryGetValue(key, out var inCache))
            inCache?.PreRemove();
        Caches.TryRemove(key, out _);
    }

    #endregion

    #region Zones

    [PrivateApi]
    public int ZoneIdOfApp(int appId, IZoneLoader tools)
    {
        try
        {
            var zones = Zones(tools);
            return zones.Single(z => z.Value.Apps.Any(a => a.Key == appId)).Key;
        }
        catch (Exception ex)
        {
            throw new($"Error trying to run {nameof(ZoneIdOfApp)}({appId}) - probably something wrong with the {nameof(appId)}", ex);
        }
    }

    /// <inheritdoc />
    public void PurgeZones() => ZoneAppCaches.Clear();

    #endregion
}
