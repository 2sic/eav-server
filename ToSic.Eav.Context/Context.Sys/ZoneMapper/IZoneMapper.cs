using ToSic.Eav.Cms.Internal.Languages;
using ToSic.Eav.Context;

namespace ToSic.Eav.Integration;

/// <summary>
/// This helps find Zone information of a Site and the other way around. 
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IZoneMapper: IHasLog
{
    /// <summary>
    /// Get the primary zoneId which belongs to the site.
    /// </summary>
    int GetZoneId(int siteId);

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