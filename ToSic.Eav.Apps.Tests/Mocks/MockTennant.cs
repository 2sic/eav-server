using ToSic.Eav.Apps.Interfaces;

namespace ToSic.Eav.Apps.Tests.Mocks
{
    public class MockTennant: ITennant
    {
        public int Id => -999;

        public string DefaultLanguage => null;

        public string Name => "MockTennant";

        public string SxcPath => "Mock";

        public bool RefactorUserIsAdmin => false;

        public string ContentPath => "MockPath";
    }
}
