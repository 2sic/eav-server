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
        public string GetThisString
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }

        [Configuration(Fallback = true)]
        public bool GetThisTrue
        {
            get => Configuration.GetThis(false);    // should return true because it was configured
            set => Configuration.SetThis(value);
        }

        [Configuration]
        public bool GetThisFalseDefault
        {
            get => Configuration.GetThis(false);
            set => Configuration.SetThis(value);
        }
        [Configuration(Fallback = false)]
        public bool GetThisFalseInitialized
        {
            get => Configuration.GetThis(false);
            set => Configuration.SetThis(value);
        }
    }
}
