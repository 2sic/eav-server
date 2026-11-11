namespace ToSic.Eav.Context.Sys.ZoneMapper;

/// <summary>
/// This helps find Zone information of a Site and the other way around. 
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ICultureStateSvc
{
    /// <summary>
    /// The cultures available on this tenant/zone combination
    /// the zone is necessary to determine what is enabled/disabled
    /// </summary>
    /// <returns></returns>
    List<ISiteLanguageState> CulturesWithState(ISite site);

}
