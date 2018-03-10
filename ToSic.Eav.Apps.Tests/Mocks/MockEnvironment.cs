using ToSic.Eav.Apps.Interfaces;

namespace ToSic.Eav.Apps.Tests.Mocks
{
    public class MockEnvironment: IEnvironment
    {
        public IZoneMapper ZoneMapper => new MockZoneMapper();

        public IUser User => null;

        public IPagePublishing PagePublishing => null;

        public string MapPath(string virtualPath) => "MockMapped";
    }
}
