using ToSic.Eav.Context;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Mocks;

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