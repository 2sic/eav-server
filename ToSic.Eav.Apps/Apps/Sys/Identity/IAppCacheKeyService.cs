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
