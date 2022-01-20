using System.Collections.Generic;
using ToSic.Eav.Context;
using ToSic.Eav.Logging;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Tests.Mocks
{
    public class MockZoneMapper: HasLog, IZoneMapper
    {
        public int GetZoneId(int siteId) => -1;

        public ISite SiteOfZone(int zoneId) => new MockSite();
        public ISite SiteOfApp(int appId) => new MockSite();

        public List<ISiteLanguageState> CulturesWithState(ISite site) => new List<ISiteLanguageState>();

        public MockZoneMapper() : base("Tst.MckZM") { }

        public IZoneMapper Init(ILog parent)
        {
            Log.LinkTo(parent);
            return this;
        }
    }
}
