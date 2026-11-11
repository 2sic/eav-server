namespace ToSic.Eav.Context.Sys.ZoneMapper;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IZoneOfSiteSvc
{
    /// <summary>
    /// Get the primary zoneId which belongs to the site.
    /// </summary>
    int GetZoneId(int siteId);
}
