namespace ToSic.Eav.Apps.Sys;

[InternalApi_DoNotUse_MayChangeWithoutNotice("Runtime key is platform-specific.")]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal sealed class RuntimeKeyServiceDefault : IRuntimeKeyService
{
    private const string Prefix = "ari"; // app runtime identifier

    public string AppRuntimeKey(IAppIdentity appIdentity)
        => $"{Prefix}{appIdentity.ZoneId}-{appIdentity.AppId}";
}
