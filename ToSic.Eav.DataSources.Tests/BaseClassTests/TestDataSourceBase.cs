namespace ToSic.Eav.DataSourceTests.BaseClassTests;

public class TestDataSourceBase: Eav.DataSource.DataSourceBase
{

    // make public for testing, otherwise protected...
    public void ConfigMask(string key, string mask) => base.ConfigMask(key, mask);

    public TestDataSourceBase(MyServices services) : base(services, "Tst.Test")
    {
    }
        
}