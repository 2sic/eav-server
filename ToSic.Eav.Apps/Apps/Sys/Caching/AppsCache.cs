using System.Collections.Concurrent;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Persistence.Sys.AppState;
using ToSic.Eav.Sys;
using ToSic.Sys.Locking;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Sys.Caching;

/// <summary>
/// The default Apps Cache system running on a normal environment.
/// </summary>
[PrivateApi]
internal class AppsCache(IAppCacheKeyService appCacheKeyService)
    : IAppsCache //, IAppsCacheSwitchable
{
    #region SwitchableService

    public const string DefaultNameId = "DefaultCache";

    //public  string NameId => DefaultNameId;

    //public bool IsViable() => true;

    //public int Priority => 1;

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
    private static readonly IDictionary<string, IAppStateCache> Caches = new Dictionary<string, IAppStateCache>();

    /// <inheritdoc />
    public bool Has(IAppIdentity app) => Caches.ContainsKey(appCacheKeyService.AppCacheKey(app));

    ///// <inheritdoc />
    //private bool Has(string cacheKey) => Caches.ContainsKey(cacheKey);

    [PrivateApi]
    public void Add(IAppStateCache appState) => Set(appCacheKeyService.AppCacheKey(appState), appState);

    /// <inheritdoc />
    private void Set(string key, IAppStateCache item)
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

    ///// <inheritdoc />
    //private IAppStateCache Get(string key) => Caches[key];

    /// <inheritdoc />
    public IAppStateCache Get(IAppIdentity app, IAppLoaderTools tools)
        => GetOrBuild(tools, app);


    /// <inheritdoc />
    public void Load(IAppIdentity app, string primaryLanguage, IAppLoaderTools tools)
        => GetOrBuild(tools, app, primaryLanguage);

    ///// <inheritdoc />
    //protected void Remove(string key) => Caches.Remove(key);    // returns false if key was not found (no Exception)

    #endregion

    #region GetOrBuild / Lock

    private IAppStateCache GetOrBuild(IAppLoaderTools tools, IAppIdentity appIdentity, string? primaryLanguage = null)
    {
        if (appIdentity.ZoneId == 0 || appIdentity.AppId == KnownAppsConstants.AppIdEmpty)
            return null!;

        var cacheKey = appCacheKeyService
                           .AppCacheKey(appIdentity)
                           .NullIfNoValue()
                       ?? throw new InvalidOperationException($"No cache key available for appIdentity: {appIdentity.Show()}");

        //IAppStateCache? appState = null;
        Caches.TryGetValue(cacheKey, out var appState);
        if (appState != null)
            return appState;

        // create lock to prevent parallel initialization
        var lockKey = LoadLocks.Get(cacheKey);
        lock (lockKey)
        {
            // now that lock is free, it could have been initialized, so re-check
            Caches.TryGetValue(cacheKey, out appState);
            if (appState != null)
                return appState;

            // Init EavSqlStore once
            var loader = tools.RepositoryLoader(null);
            if (primaryLanguage != null)
                loader.PrimaryLanguage = primaryLanguage;
            appState = loader.AppState(appIdentity.AppId, new CodeRefTrail().AddMessage($"App: {cacheKey}; DateTime: {DateTime.Now:O}; 2sxc Version: {EavSystemInfo.VersionString}"));
            Set(cacheKey, appState);
        }

        return appState;
    }

    /// <summary>
    /// List of locks, to ensure that each app locks the loading process separately
    /// </summary>
    private static readonly NamedLocks LoadLocks = new();


    #endregion

    #region Update

    /// <inheritdoc />
    public virtual void Update(IAppIdentity appIdentity, IEnumerable<int> entities, ILog log, IAppLoaderTools tools)
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
        Caches.Remove(key);
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
