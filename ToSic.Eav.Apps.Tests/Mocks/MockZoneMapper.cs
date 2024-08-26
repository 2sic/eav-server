using System.Collections.Generic;
using ToSic.Eav.Cms.Internal.Languages;
using ToSic.Eav.Context;
using ToSic.Eav.Integration;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Tests.Mocks;

public class MockZoneMapper() : ServiceBase("Tst.MckZM"), IZoneMapper
{
    public int GetZoneId(int siteId) => -1;

    public ISite SiteOfZone(int zoneId) => new MockSite();
    public ISite SiteOfApp(int appId) => new MockSite();

    public List<ISiteLanguageState> CulturesWithState(ISite site) => [];
    public List<ISiteLanguageState> CulturesEnabledWithState(ISite site) => [];
}