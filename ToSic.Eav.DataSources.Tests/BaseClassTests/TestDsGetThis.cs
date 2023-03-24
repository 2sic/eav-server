using ToSic.Eav.DataSources;

namespace ToSic.Eav.DataSourceTests.BaseClassTests
{
    public class TestDsGetThis : TestDataSourceBase
    {
        public const string ExpectedGetThisString = "ok";

        public TestDsGetThis(MyServices services) : base(services)
        {
        }
        [Configuration(Fallback = ExpectedGetThisString)]
        public string GetThisString => Configuration.GetThis();

        [Configuration(Fallback = true)]
        public bool GetThisTrue => Configuration.GetThis(false); // should return true because it was configured

        [Configuration]
        public bool GetThisFalseDefault => Configuration.GetThis(false);
        [Configuration(Fallback = false)]
        public bool GetThisFalseInitialized => Configuration.GetThis(false);
    }
}
