using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Apps.Tests.Mocks
{
    public class MockEnvironment: Interfaces.IAppEnvironment
    {
        public IZoneMapper ZoneMapper => new MockZoneMapper();

        public IUser User => null;

        public IPagePublishing PagePublishing => null;

        public string MapAppPath(string virtualPath) => "MockMapped";


        public string DefaultLanguage => "en-US";
    }
}
