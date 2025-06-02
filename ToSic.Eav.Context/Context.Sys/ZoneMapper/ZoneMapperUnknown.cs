using ToSic.Eav.Apps;
using ToSic.Eav.Cms.Internal.Languages;
using ToSic.Eav.Context;
using ToSic.Eav.Internal.Unknown;
#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.Integration;

internal class ZoneMapperUnknown(IAppsCatalog appsCatalog, WarnUseOfUnknown<ZoneMapperUnknown> _, Generator<ISite> site)
    : ZoneMapperBase(appsCatalog, $"{LogScopes.NotImplemented}.ZonMap", connect: []), IIsUnknown
{
    public override int GetZoneId(int siteId) => siteId;
        
    public override ISite SiteOfZone(int zoneId) => site.New().Init(zoneId, null);

    public override List<ISiteLanguageState> CulturesWithState(ISite site) => [];
}