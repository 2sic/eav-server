using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Tests.Mocks
{
    public class MockTenant: ITenant
    {
        public int Id => -999;

        public ITenant Init(int tenantId)
        {
            return this;
        }

        public string DefaultLanguage => null;

        public string Name => "MockTenant";
        public string Url => "Mock.org/root";

        public string AppsRootPhysical => "Mock";
        public string AppsRootPhysicalFull => "mock full";
        public string AppsRootLink => "Mock/Mock/Mock";

        public bool RefactorUserIsAdmin => false;

        public string ContentPath => "MockPath";
        public int ZoneId => -999;
    }
}
