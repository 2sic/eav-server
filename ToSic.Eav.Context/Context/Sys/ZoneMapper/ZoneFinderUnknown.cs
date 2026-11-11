namespace ToSic.Eav.Context.Sys.ZoneMapper;

#pragma warning disable CS9113
internal class ZoneFinderUnknown(WarnUseOfUnknown<ZoneFinderUnknown> _)
    : ServiceBase($"{LogScopes.NotImplemented}.ZonMap", connect: []), IZoneFinder, IIsUnknown
{
    public int GetZoneId(int siteId) => siteId;
}
