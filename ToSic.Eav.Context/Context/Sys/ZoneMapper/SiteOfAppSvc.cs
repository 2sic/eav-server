using ToSic.Eav.Apps;

namespace ToSic.Eav.Context.Sys.ZoneMapper;

public class SiteOfAppSvc(ISiteOfZoneSvc siteOfZoneSvc, IAppsCatalog appsCatalog)
    : ServiceBase("Eav.Ap2Zn", connect: [siteOfZoneSvc, appsCatalog]), ISiteOfAppSvc
{
    /// <inheritdoc />
    public ISite SiteOfApp(int appId)
    {
        var l = Log.Fn<ISite>($"{appId}");
        var appIdentifier = appsCatalog.AppIdentity(appId);
        var site = siteOfZoneSvc.SiteOfZone(appIdentifier.ZoneId);
        return l.Return(site);
    }
}
