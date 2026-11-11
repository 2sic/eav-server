namespace ToSic.Eav.Context.Sys.ZoneMapper;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppSiteService
{
    ISite SiteOfApp(int appId);
}
