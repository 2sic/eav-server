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

        #endregion

        public IUser User => null;


        public string DefaultLanguage => "en-US";

    }
}
