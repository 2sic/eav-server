using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Cms.Internal.Languages;
using ToSic.Eav.Context;
using ToSic.Eav.Internal.Unknown;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Integration;

internal class ZoneMapperUnknown(IAppStates appStates, WarnUseOfUnknown<ZoneMapperUnknown> _, Generator<ISite> site) : ZoneMapperBase(appStates, $"{LogScopes.NotImplemented}.ZonMap"), IIsUnknown
{
    public override int GetZoneId(int siteId) => siteId;
        
    public override ISite SiteOfZone(int zoneId) => site.New().Init(zoneId, null);

    public override List<ISiteLanguageState> CulturesWithState(ISite site) => [];
}