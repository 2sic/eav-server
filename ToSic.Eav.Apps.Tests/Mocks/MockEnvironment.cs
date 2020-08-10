using ToSic.Eav.Logging;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Tests.Mocks
{
    public class MockEnvironment: HasLog, IAppEnvironment
    {
        #region Constructors

        public MockEnvironment() : base("Tst.MckEnv") { }
        public IAppEnvironment Init(ILog parent)
        {
            Log.LinkTo(parent);
            return this;
        }
        #endregion

        public IZoneMapper ZoneMapper => new MockZoneMapper();

        public IUser User => null;

        public IPagePublishing PagePublishing => null;

        public string MapPath(string virtualPath) => "MockMapped";


        public string DefaultLanguage => "en-US";

    }
}
