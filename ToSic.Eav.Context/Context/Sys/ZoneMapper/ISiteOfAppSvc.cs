namespace ToSic.Eav.Context.Sys.ZoneMapper;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ISiteOfAppSvc
{
    ISite SiteOfApp(int appId);
}
