namespace ToSic.Eav.DataSource.Configuration;

public class TestDataSourceBase(DataSourceBase.MyServices services)
    : DataSourceBase(services, "Tst.Test")
{

    // make public for testing, otherwise protected...
    public void ConfigMask(string key, string mask) => base.ConfigMask(key, mask);
}