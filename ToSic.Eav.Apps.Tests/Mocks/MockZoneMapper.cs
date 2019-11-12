using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Apps.Environment;
using ToSic.Eav.Environment;

namespace ToSic.Eav.Apps.Tests.Mocks
{
    public class MockZoneMapper: IZoneMapper
    {
        public int GetZoneId(int tenantId) => -1;

        public int GetZoneId(ITenant tenant) => -999;

        public ITenant Tenant(int zoneId) => new MockTenant();

        public List<TempTempCulture> CulturesWithState(int tenantId, int zoneId) => new List<TempTempCulture>();
    }
}
