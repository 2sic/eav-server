namespace ToSic.Eav.Context.Sys.ZoneMapper;

#pragma warning disable CS9113
internal class SiteOfZoneSvcUnknown(WarnUseOfUnknown<SiteOfZoneSvcUnknown> _, Generator<ISite> siteFactory)
    : ServiceBase($"{LogScopes.NotImplemented}.ZonMap", connect: []), ISiteOfZoneSvc, IIsUnknown
{
    public ISite SiteOfZone(int zoneId) => siteFactory.New();

}
