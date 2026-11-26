using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Sys;

[InternalApi_DoNotUse_MayChangeWithoutNotice("Runtime key is platform-specific.")]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal sealed class AppCacheKeyServiceDefault : IAppCacheKeyService
{
    public string AppCacheKey(IAppIdentity appIdentity)
    {
        // Short-Circuit if we already have a runtime key
        return appIdentity is IAppStateCache { CacheKey: { } runtimeKey } && runtimeKey.HasValue()
            ? runtimeKey
            // Otherwise use a trivial schema
            : $"z{appIdentity.ZoneId:D3}-a{appIdentity.AppId:D5}"; // app runtime identifier
    }
}

internal static class AppCacheKeyHelpers
{
    /// <summary>
    /// Gets the KeySchema used to store values for a specific Zone and App. Must contain {0} for ZoneId and {1} for AppId
    /// </summary>
    [PrivateApi]
    public const string CacheKeySchema = "Z{0}A{1}";

    public static string FallbackCacheKey(this IAppIdentity appIdentity)
        => string.Format(CacheKeySchema, appIdentity.ZoneId, appIdentity.AppId);
}
