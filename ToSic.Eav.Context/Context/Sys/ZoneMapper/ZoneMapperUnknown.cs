using ToSic.Eav.Apps;

#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.Context.Sys.ZoneMapper;

internal class ZoneMapperUnknown(IAppsCatalog appsCatalog, WarnUseOfUnknown<ZoneMapperUnknown> _, Generator<ISite> site)
    : ZoneMapperBase(appsCatalog, $"{LogScopes.NotImplemented}.ZonMap", connect: []), IIsUnknown
{
    public override int GetZoneId(int siteId) => siteId;
        
    public override ISite SiteOfZone(int zoneId) => site.New().Init(zoneId, null);

    public override List<ISiteLanguageState> CulturesWithState(ISite site) => [];
}