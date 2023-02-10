namespace ToSic.Eav.DataSourceTests.BaseClassTests
{
    public class TestDataSourceBase: DataSources.DataSource
    {

        // make public for testing, otherwise protected...
        public void ConfigMask(string key, string mask) => base.ConfigMask(key, mask);
        public void ConfigMask(string key) => base.ConfigMask(key);

        public TestDataSourceBase(Dependencies dependencies) : base(dependencies, "Tst.Test")
        {
        }
        
    }
}
