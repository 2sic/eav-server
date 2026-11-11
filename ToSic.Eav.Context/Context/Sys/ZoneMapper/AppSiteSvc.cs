using ToSic.Eav.Apps;

namespace ToSic.Eav.Context.Sys.ZoneMapper;

public class AppSiteSvc(IZoneMapper zoneMapper, IAppsCatalog appsCatalog)
    : ServiceBase("Eav.Ap2Zn", connect: [zoneMapper, appsCatalog]), IAppSiteService
{
    /// <inheritdoc />
    public ISite SiteOfApp(int appId)
    {
        var l = Log.Fn<ISite>($"{appId}");
        var appIdentifier = appsCatalog.AppIdentity(appId);
        var site = zoneMapper.SiteOfZone(appIdentifier.ZoneId);
        return l.Return(site);
    }
}
