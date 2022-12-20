using System;
using System.Collections.Generic;
using ToSic.Eav.Apps.Languages;
using ToSic.Eav.Context;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;
using ToSic.Lib.DI;

namespace ToSic.Eav.Apps.Run
{
    public class ZoneMapperUnknown: ZoneMapperBase, IIsUnknown
    {
        private readonly Generator<ISite> _site;

        public ZoneMapperUnknown(IAppStates appStates, WarnUseOfUnknown<ZoneMapperUnknown> warn, Generator<ISite> site) 
            : base(appStates, $"{LogNames.NotImplemented}.ZonMap")
        {
            _site = site;
        }

        public override int GetZoneId(int siteId) => siteId;
        
        public override ISite SiteOfZone(int zoneId) => _site.New().Init(zoneId, null);

        public override List<ISiteLanguageState> CulturesWithState(ISite site) => new List<ISiteLanguageState>();
    }
}
