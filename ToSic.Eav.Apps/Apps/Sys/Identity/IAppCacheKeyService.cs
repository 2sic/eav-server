using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Sys;

/// <summary>
/// Service to build cache keys for apps.
/// </summary>
/// <remarks>
/// Since platforms like Oqtane have different mechanisms for splitting apps/tenants,
/// the cache key could collide since it could result in the same AppId being used in different tenants.
/// This service allows to build a runtime-specific key for an app identity.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice("Runtime key is platform-specific.")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppCacheKeyService
{
    /// <summary>
    /// Build a runtime-specific key for an app identity.
    /// </summary>
    string AppCacheKey(IAppIdentity appIdentity);
}

public static class IAppCacheKeyServiceExtensions
{
    // Returns true only when we have a tenant-aware runtime key to safely partition zone caches.
    public static bool TryZoneCacheKey(this IAppCacheKeyService appCacheKeyService, out string cacheKey)
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
}
