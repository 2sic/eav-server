using ToSic.Eav.Logging;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Tests.Mocks
{
    public class MockEnvironment: HasLog, IEnvironment
    {
        #region Constructors

        public MockEnvironment() : base("Tst.MckEnv") { }
        public IEnvironment Init(ILog parent)
        {
            Log.LinkTo(parent);
            return this;
        }

        public string MapPath(string path)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        public IZoneMapper ZoneMapper => new MockZoneMapper();

        public IUser User => null;

        public IPagePublishing PagePublishing => null;

        //public string MapPath(string virtualPath) => "MockMapped";


        public string DefaultLanguage => "en-US";

    }
}
