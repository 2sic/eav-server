namespace ToSic.Eav.Apps.Sys;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ZoneIdentityExtensions
{
    public static IAppIdentity ToAppIdentity(this IZoneIdentity zoneIdentity, int appId) =>
        new AppIdentity(zoneIdentity.ZoneId, appId);
}
