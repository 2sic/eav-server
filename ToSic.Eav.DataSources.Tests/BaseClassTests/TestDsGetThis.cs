namespace ToSic.Eav.DataSourceTests.BaseClassTests
{
    public class TestDsGetThis : TestDataSourceBase
    {
        public const string ExpectedGetThisString = "ok";

        public TestDsGetThis(Dependencies dependencies) : base(dependencies)
        {
            ConfigMask(nameof(GetThisString) + "||" + ExpectedGetThisString);
            ConfigMask(nameof(GetThisTrue) + "||true");
            ConfigMask(nameof(GetThisFalseDefault));
            ConfigMask(nameof(GetThisFalseInitialized) + "||false");
        }

        public string GetThisString
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }

        public bool GetThisTrue
        {
            get => Configuration.GetThis(false);    // should return true because it was configured
            set => Configuration.SetThis(value);
        }
        public bool GetThisFalseDefault
        {
            get => Configuration.GetThis(false);
            set => Configuration.SetThis(value);
        }
        public bool GetThisFalseInitialized
        {
            get => Configuration.GetThis(false);
            set => Configuration.SetThis(value);
        }
    }
}
