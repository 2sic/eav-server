using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys;
using ToSic.Eav.Context.Sys.ZoneMapper;

namespace ToSic.Eav.Apps.Mocks;

public class MockCultureStateSvc() : ServiceBase("Tst.MckZM"), ICultureStateSvc
{
    public ISite SiteOfZone(int zoneId) => new MockSite();

    public List<ISiteLanguageState> CulturesWithState(ISite site) => [];
    public List<ISiteLanguageState> CulturesEnabledWithState(ISite site) => [];
}