namespace ToSic.Eav.Apps;

// ReSharper disable once InconsistentNaming
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class IAppIdentityExtensions
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string Show(this IAppIdentity appId) => $"{appId.ZoneId}/{appId.AppId}";

    public static IAppIdentityPure PureIdentity(this IAppIdentity appId) => new AppIdentityPure(appId.ZoneId, appId.AppId);
}