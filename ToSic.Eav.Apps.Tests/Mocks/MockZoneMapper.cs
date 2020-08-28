using System.Collections.Generic;
using ToSic.Eav.Logging;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Tests.Mocks
{
    public class MockZoneMapper: HasLog, IZoneMapper
    {
        public int GetZoneId(int tenantId) => -1;

        public int GetZoneId(ITenant tenant) => -999;
        public IAppIdentity IdentityFromTenant(int tenantId, int appId) 
            => new AppIdentity(-1, appId);

        public ITenant TenantOfZone(int zoneId) => new MockTenant();
        public ITenant TenantOfApp(int appId) => new MockTenant();

        public List<TempTempCulture> CulturesWithState(int tenantId, int zoneId) => new List<TempTempCulture>();

        public MockZoneMapper() : base("Tst.MckZM") { }

        public IZoneMapper Init(ILog parent)
        {
            Log.LinkTo(parent);
            return this;
        }
    }
}
