namespace ToSic.Eav.Context.Sys.ZoneMapper;

#pragma warning disable CS9113
internal class ZoneOfSiteSvcUnknown(WarnUseOfUnknown<ZoneOfSiteSvcUnknown> _)
    : ServiceBase($"{LogScopes.NotImplemented}.ZonMap", connect: []), IZoneOfSiteSvc, IIsUnknown
{
    public int GetZoneId(int siteId) => siteId;
}
