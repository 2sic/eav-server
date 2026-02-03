namespace ToSic.Eav.Apps.Sys;

[InternalApi_DoNotUse_MayChangeWithoutNotice("Runtime key is platform-specific.")]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal sealed class RuntimeKeyServiceDefault : IRuntimeKeyService
{
    public string AppRuntimeKey(IAppIdentity appIdentity)
        => $"z{appIdentity.ZoneId:D3}-a{appIdentity.AppId:D5}"; // app runtime identifier
}
