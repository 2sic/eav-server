using ToSic.Eav.Interfaces;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Tests.Mocks
{
    public class MockEnvironment: IAppEnvironment
    {
        public IZoneMapper ZoneMapper => new MockZoneMapper();

        public IUser User => null;

        public IPagePublishing PagePublishing => null;

        public string MapAppPath(string virtualPath) => "MockMapped";


        public string DefaultLanguage => "en-US";
    }
}
