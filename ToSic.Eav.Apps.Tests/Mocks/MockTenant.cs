using ToSic.Eav.Environment;

namespace ToSic.Eav.Apps.Tests.Mocks
{
    public class MockTenant: ITenant
    {
        public int Id => -999;

        public string DefaultLanguage => null;

        public string Name => "MockTenant";

        public string SxcPath => "Mock";

        public bool RefactorUserIsAdmin => false;

        public string ContentPath => "MockPath";
    }
}
