using ToSic.Eav.Context;

namespace ToSic.Eav.Apps.Tests.Mocks
{
    public class MockSite : ISite
    {
        public int Id => -999;

        public ISite Init(int tenantId)
        {
            return this;
        }

        public string DefaultLanguage => null;

        public string Name => "MockTenant";
        public string Url => "https://ock.org/root";
        public string UrlRoot => "mock.org/root";

        public string AppsRootPhysical => "Mock";
        public string AppsRootPhysicalFull => "mock full";
        public string AppAssetsLinkTemplate => "Mock/Mock/Mock";
        public string ContentPath => "MockPath";
        public int ZoneId => -999;
        public string CurrentCultureCode { get; }
        public string DefaultCultureCode { get; }
    }
}
