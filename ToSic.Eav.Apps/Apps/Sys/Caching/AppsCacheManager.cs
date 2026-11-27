using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Persistence.Sys.AppState;

namespace ToSic.Eav.Apps.Sys.Caching;

/// <summary>
/// The local and default Apps Cache system running on a normal environment.
/// </summary>
/// <remarks>
/// To replace this, you can implement your own IAppsCache and register it in the DI system.
///
/// 1. Either create a full replacement implementing <see cref="IAppsCacheManager"/> and register it both as <see cref="IAppsCacheManager"/> and as the key it's using.
/// 2. Or create a lightweight wrapper and inject this as <see cref="IAppsCacheManager"/> (it is also registered in DI).
/// </remarks>
[PrivateApi]
internal class AppsCacheManager(IAppCacheKeyService appCacheKeyService, IAppsCacheInMemory inMemory, LazySvc<IAppsAndZonesLoader> loaders)
    : IAppsCacheManager
{
    #region Keyed Service Identity

    public const string DefaultNameId = "DefaultCache";

    /// <summary>
    /// Identifies this cache to check if the one currently used matches this one.
    /// </summary>
    public string NameId => DefaultNameId;

    #endregion

    public IReadOnlyDictionary<int, Zone> Zones()
    {
        // Cache zones per-tenant runtime key; if unavailable, skip caching to avoid cross-tenant bleed.
        if (!appCacheKeyService.TryZoneCacheKey(out var cacheKey))
            return new ZoneCachePresetHelper().AddPresetZone(loaders.Value.Zones());

        // Lazy ensures only one LoadZones per tenant key, even under concurrency.
        var lazy = inMemory.ZonesWithApps.GetOrAdd(cacheKey, _ => new(() => new ZoneCachePresetHelper().AddPresetZone(loaders.Value.Zones())));
        return lazy.Value;
    }

    #region The cache-variable + HasCacheItem, SetCacheItem, Get, Remove


    /// <inheritdoc />
    public bool Has(IAppIdentity app)
        => inMemory.AppStates.ContainsKey(appCacheKeyService.AppCacheKey(app));

    [PrivateApi]
    public void Add(IAppStateCache appState)
        => inMemory.AppStates.Set(appCacheKeyService.AppCacheKey(appState), appState);


    /// <inheritdoc />
    public IAppStateCache Get(IAppIdentity app)
        => inMemory.AppStates.GetOrBuild(appCacheKeyService, loaders, app);

    #endregion


    #region Update

    /// <inheritdoc />
    public void Update(IAppIdentity appIdentity, IEnumerable<int> entities, ILog log)
    {
        var entityIds = entities.ToArray();
        var l = log.Fn($"{appIdentity}, {entityIds.Length} entities");
        // if it's not cached yet, ignore the request as partial update won't be necessary
        if (!Has(appIdentity))
            l.Done("not cached, won't update; probably updating an app in the process of being created.");
        
        // It should be here now - verify, then update
        if (Get(appIdentity) is not { } appState)
            throw new NullReferenceException("AppState was returned null. This should never happen.");

        loaders.Value.Update(appState, AppStateLoadSequence.ItemLoad, codeRefTrail: new(), entityIds: [.. entityIds]);
        l.Done("ok");
    }

    #endregion


    #region Purge Cache

    /// <inheritdoc />
    public void Purge(IAppIdentity app)
    {
        var key = appCacheKeyService.AppCacheKey(app);
        if (inMemory.AppStates.TryGetValue(key, out var inCache))
            inCache?.PreRemove();
        inMemory.AppStates.TryRemove(key, out _);
    }

    #endregion

    #region Zones

    [PrivateApi]
    public int ZoneIdOfApp(int appId)
    {
        try
        {
            var zones = Zones();
            return zones.Single(z => z.Value.Apps.Any(a => a.Key == appId)).Key;
        }
        catch (Exception ex)
        {
            throw new($"Error trying to run {nameof(ZoneIdOfApp)}({appId}) - probably something wrong with the {nameof(appId)}", ex);
        }
    }

    /// <inheritdoc />
    public void PurgeZones() => inMemory.ZonesWithApps.Clear();

    #endregion
}
