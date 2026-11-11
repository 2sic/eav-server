#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.Context.Sys.ZoneMapper;

internal class ZoneMapperUnknown(WarnUseOfUnknown<ZoneMapperUnknown> _, Generator<ISite> siteFactory)
    : ServiceBase($"{LogScopes.NotImplemented}.ZonMap", connect: []), IZoneMapper, IIsUnknown
{
    public ISite SiteOfZone(int zoneId) => siteFactory.New();

    public List<ISiteLanguageState> CulturesWithState(ISite site) => [];
}
