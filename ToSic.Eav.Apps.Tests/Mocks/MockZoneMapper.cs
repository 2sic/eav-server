using System.Collections.Generic;
using ToSic.Eav.Logging;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Tests.Mocks
{
    public class MockZoneMapper: HasLog, IZoneMapper
    {
        public int GetZoneId(int tenantId) => -1;

        public int GetZoneId(ISite site) => -999;
        public IAppIdentity IdentityFromSite(int tenantId, int appId) 
            => new AppIdentity(-1, appId);

        public ISite SiteOfZone(int zoneId) => new MockTenant();
        public ISite TenantOfApp(int appId) => new MockTenant();

        public List<TempTempCulture> CulturesWithState(int tenantId, int zoneId) => new List<TempTempCulture>();

        public MockZoneMapper() : base("Tst.MckZM") { }

        public IZoneMapper Init(ILog parent)
        {
            Log.LinkTo(parent);
            return this;
        }
    }
}
