using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Sys;
using ToSic.Sys.Locking;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Sys.Caching;

public static class AppsCacheLockExtensions
{
    public static void Set(this IDictionary<string, IAppStateCache> caches, string key, IAppStateCache item)
    {
        try
        {
            // add or create
            // 2018-03-28 added lock - because I assume that's the cause of the random errors sometimes on system-load - see #1498
            lock (caches)
            {
                caches[key] = item;
            }
        }
        catch (Exception ex)
        {
            // unclear why this pops up sometime...if it would also hit on live, so I'm adding some more info
            throw new("issue with setting cache item - key is '" + key + "' and cache is null =" +
                      (caches == null!) + " and item is null=" + (item == null!), ex);
        }
    }

    #region GetOrBuild / Lock

    public static IAppStateCache GetOrBuild(this IDictionary<string, IAppStateCache> caches, IAppCacheKeyService appCacheKeyService, IAppLoaderTools tools, IAppIdentity appIdentity, string? primaryLanguage = null)
    {
        if (appIdentity.ZoneId == 0 || appIdentity.AppId == KnownAppsConstants.AppIdEmpty)
            return null!;

        var cacheKey = appCacheKeyService
                           .AppCacheKey(appIdentity)
                           .NullIfNoValue()
                       ?? throw new InvalidOperationException($"No cache key available for appIdentity: {appIdentity.Show()}");

        //IAppStateCache? appState = null;
        caches.TryGetValue(cacheKey, out var appState);
        if (appState != null)
            return appState;

        // create lock to prevent parallel initialization
        var lockKey = LoadLocks.Get(cacheKey);
        lock (lockKey)
        {
            // now that lock is free, it could have been initialized, so re-check
            caches.TryGetValue(cacheKey, out appState);
            if (appState != null)
                return appState;

            // Init EavSqlStore once
            var loader = tools.RepositoryLoader(null);
            if (primaryLanguage != null)
                loader.PrimaryLanguage = primaryLanguage;
            appState = loader.AppState(appIdentity.AppId, new CodeRefTrail().AddMessage($"App: {cacheKey}; DateTime: {DateTime.Now:O}; 2sxc Version: {EavSystemInfo.VersionString}"));
            caches.Set(cacheKey, appState);
        }

        return appState;
    }

    /// <summary>
    /// List of locks, to ensure that each app locks the loading process separately
    /// </summary>
    private static readonly NamedLocks LoadLocks = new();

    #endregion

}