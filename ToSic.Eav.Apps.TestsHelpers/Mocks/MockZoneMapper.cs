using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys;
using ToSic.Eav.Context.Sys.ZoneMapper;

namespace ToSic.Eav.Apps.Mocks;

public class MockZoneMapper() : ServiceBase("Tst.MckZM"), IZoneMapper
{
    public int GetZoneId(int siteId, int? tenantId = null) => -1;

    public ISite SiteOfZone(int zoneId) => new MockSite();
    public ISite SiteOfApp(int appId) => new MockSite();

    public List<ISiteLanguageState> CulturesWithState(ISite site) => [];
    public List<ISiteLanguageState> CulturesEnabledWithState(ISite site) => [];
}