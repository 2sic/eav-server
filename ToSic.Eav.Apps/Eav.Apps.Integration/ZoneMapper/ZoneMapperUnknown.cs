using System.Collections.Generic;
using ToSic.Eav.Apps.Languages;
using ToSic.Eav.Context;
using ToSic.Eav.Internal.Unknown;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Run;

internal class ZoneMapperUnknown: ZoneMapperBase, IIsUnknown
{
    private readonly Generator<ISite> _site;

    public ZoneMapperUnknown(IAppStates appStates, WarnUseOfUnknown<ZoneMapperUnknown> _, Generator<ISite> site) 
        : base(appStates, $"{LogScopes.NotImplemented}.ZonMap")
    {
        _site = site;
    }

    public override int GetZoneId(int siteId) => siteId;
        
    public override ISite SiteOfZone(int zoneId) => _site.New().Init(zoneId, null);

    public override List<ISiteLanguageState> CulturesWithState(ISite site) => [];
}