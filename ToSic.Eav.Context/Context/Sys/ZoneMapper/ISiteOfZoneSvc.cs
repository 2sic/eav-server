namespace ToSic.Eav.Context.Sys.ZoneMapper;

/// <summary>
/// Get the site of a zone.
/// </summary>
public interface ISiteOfZoneSvc
{
    /// <summary>
    /// Find the site of a Zone
    /// </summary>
    ISite SiteOfZone(int zoneId);
}
