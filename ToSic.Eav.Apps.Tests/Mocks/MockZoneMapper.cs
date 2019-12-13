using System.Collections.Generic;
using ToSic.Eav.Apps.Environment;
using ToSic.Eav.Environment;

namespace ToSic.Eav.Apps.Tests.Mocks
{
    public class MockZoneMapper: IZoneMapper
    {
        public int GetZoneId(int tenantId) => -1;

        public int GetZoneId(ITenant tenant) => -999;
        public IAppIdentity IdentityFromTenant(int tenantId, int appId) 
            => new AppIdentity(-1, appId);

        public ITenant Tenant(int zoneId) => new MockTenant();

        public List<TempTempCulture> CulturesWithState(int tenantId, int zoneId) => new List<TempTempCulture>();
    }
}
