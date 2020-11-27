using System.Collections.Generic;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    public class UnknownZoneMapper: ZoneMapperBase
    {
        public UnknownZoneMapper(string logName) : base(logName)  {  }

        public override int GetZoneId(int siteId) => siteId;

        public override ISite SiteOfZone(int zoneId) => new UnknownSite().Init(zoneId);

        public override List<TempTempCulture> CulturesWithState(int tenantId, int zoneId) => new List<TempTempCulture>();
    }
}
