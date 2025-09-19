namespace ToSic.Eav.Context.Sys.ZoneMapper;

/// <summary>
/// This helps find Zone information of a Site and the other way around. 
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IZoneMapper: IHasLog
{
    /// <summary>
    /// Get the primary zoneId which belongs to the site.
    /// Optional tenantId is provided for multi-database or multi-tenant hosting where both tenant and site are distinct.
    /// </summary>
    int GetZoneId(int siteId, int? tenantId = null);

    /// <summary>
    /// Find the site of a Zone
    /// </summary>
    ISite SiteOfZone(int zoneId);

    /// <summary>
    /// Find the site of an App
    /// </summary>
    ISite SiteOfApp(int appId);

    /// <summary>
    /// The cultures available on this tenant/zone combination
    /// the zone is necessary to determine what is enabled/disabled
    /// </summary>
    /// <returns></returns>
    List<ISiteLanguageState> CulturesWithState(ISite site);

    List<ISiteLanguageState> CulturesEnabledWithState(ISite site);
}