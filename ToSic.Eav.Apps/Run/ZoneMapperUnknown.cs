using System;
using System.Collections.Generic;
using ToSic.Eav.Apps.Languages;
using ToSic.Eav.Context;
using ToSic.Eav.DI;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;

namespace ToSic.Eav.Apps.Run
{
    public class ZoneMapperUnknown: ZoneMapperBase, IIsUnknown
    {
        public ZoneMapperUnknown(IAppStates appStates, WarnUseOfUnknown<ZoneMapperUnknown> warn, IServiceProvider sp) 
            : base(appStates, $"{LogNames.NotImplemented}.ZonMap")
        {
            _sp = sp;
        }
        private readonly IServiceProvider _sp;

        public override int GetZoneId(int siteId) => siteId;

        public override ISite SiteOfZone(int zoneId) => _sp.Build<ISite>().Init(zoneId, null);

        public override List<ISiteLanguageState> CulturesWithState(ISite site) => new List<ISiteLanguageState>();
    }
}
