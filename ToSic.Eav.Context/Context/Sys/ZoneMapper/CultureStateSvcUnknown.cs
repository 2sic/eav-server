#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.Context.Sys.ZoneMapper;

internal class CultureStateSvcUnknown(WarnUseOfUnknown<CultureStateSvcUnknown> _, Generator<ISite> siteFactory)
    : ServiceBase($"{LogScopes.NotImplemented}.ZonMap", connect: []), ICultureStateSvc, IIsUnknown
{
    public ISite SiteOfZone(int zoneId) => siteFactory.New();

    public List<ISiteLanguageState> CulturesWithState(ISite site) => [];
}
