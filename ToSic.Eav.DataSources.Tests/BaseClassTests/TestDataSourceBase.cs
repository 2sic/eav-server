using ToSic.Eav.DataSources;

namespace ToSic.Eav.DataSourceTests.BaseClassTests
{
    public class TestDataSourceBase: DataSourceBase
    {
        public override string LogId => "Test";

        public void ConfigMask(string key, string mask) => base.ConfigMask(key, mask);
        public void ConfigMask(string key) => base.ConfigMask(key);
    }
}
