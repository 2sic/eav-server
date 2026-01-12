namespace ToSic.Eav.Apps.Sys;

/// <summary>
/// Service to build runtime-specific cache keys for apps.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("Runtime key is platform-specific.")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IRuntimeKeyService
{
    /// <summary>
    /// Build a runtime-specific key for an app identity.
    /// </summary>
    string AppRuntimeKey(IAppIdentity appIdentity);
}
