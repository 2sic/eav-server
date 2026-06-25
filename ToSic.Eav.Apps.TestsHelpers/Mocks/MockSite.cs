using ToSic.Eav.Context;

namespace ToSic.Eav.Apps.Mocks;

// not good, should be unified, there are to similar MockSite, and ideally they should have properties which can be set as needed, similar to UserMock
public class MockSite : ISite
{
    public ISite Init(int siteId, ILog? parentLogOrNull)
    {
        return this;
    }

    public int Id => -999;

    public string DefaultLanguage => null!;

    public string Name => "MockTenant";
    public string Url => "https://ock.org/root";
    public string UrlRoot => "mock.org/root";

    public string AppsRootPhysical => "Mock";
    public string AppsRootPhysicalFull => "mock full";
    public string AppAssetsLinkTemplate => "Mock/Mock/Mock";
    public string ContentPath => "MockPath";
    public int ZoneId => -999;
    public string CurrentCultureCode { get; } = null!;
    public string DefaultCultureCode { get; } = null!;
}