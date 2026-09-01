using ToSic.Eav.Context;

namespace ToSic.Eav.Apps.Mocks;

// not good, should be unified, there are to similar MockSite, and ideally they should have properties which can be set as needed, similar to UserMock
public class MockSite : ISite
{
    public ISite Init(int siteId, ILog? parentLogOrNull) => this;

    public int Id { get; init; } = -999;

    public string DefaultLanguage { get; init; } = null!;

    public string Name { get; init; } = "MockTenant";
    public string Url { get; init; } = "https://ock.org/root";
    public string UrlRoot { get; init; } = "mock.org/root";

    public string AppsRootPhysical { get; init; } = "Mock";
    public string AppsRootPhysicalFull { get; init; } = "mock full";
    public string AppAssetsLinkTemplate { get; init; } = "Mock/Mock/Mock";
    public string ContentPath { get; init; } = "MockPath";
    public int ZoneId { get; init; } = -999;
    public string CurrentCultureCode { get; init; } = null!;
    public string DefaultCultureCode { get; init; } = null!;
}
