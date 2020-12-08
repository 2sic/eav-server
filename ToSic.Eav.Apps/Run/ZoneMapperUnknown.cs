using System.Collections.Generic;
using ToSic.Eav.Context;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    public class ZoneMapperUnknown: ZoneMapperBase, IIsUnknown
    {
        public ZoneMapperUnknown() : base($"{LogNames.NotImplemented}.ZonMap")  {  }

        public override int GetZoneId(int siteId) => siteId;

        public override ISite SiteOfZone(int zoneId) => new SiteUnknown().Init(zoneId);

        public override List<TempTempCulture> CulturesWithState(int tenantId, int zoneId) => new List<TempTempCulture>();
    }
}
