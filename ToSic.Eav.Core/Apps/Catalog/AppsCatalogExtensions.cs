namespace ToSic.Eav.Apps.Catalog;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class AppsCatalogExtensions
{

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static int GetPrimaryAppOfAppId(this IAppsCatalog appsCatalog, int appId, ILog log)
    {
        var l = log.Fn<int>($"{appId}");
        var zoneId = appsCatalog.AppIdentity(appId).ZoneId;
        var primaryIdentity = appsCatalog.PrimaryAppIdentity(zoneId);
        return l.Return(primaryIdentity.AppId, primaryIdentity.Show());
    }

}