namespace ToSic.Eav.Apps.Catalog;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class AppsCatalogExtensions
{

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static int GetPrimaryAppOfAppId(this IAppsCatalog appsCatalog, int appId, ILog log)
    {
        var l = log.Fn<int>($"{appId}");
        var zoneId = appsCatalog.AppIdentity(appId).ZoneId;
        var primaryIdentity = appsCatalog.PrimaryAppIdentity(zoneId);
        return l.Return(primaryIdentity.AppId, primaryIdentity.Show());
    }

}