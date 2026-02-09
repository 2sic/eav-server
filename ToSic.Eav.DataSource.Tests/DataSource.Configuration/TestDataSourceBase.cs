using ToSic.Eav.DataSource.Sys;

namespace ToSic.Eav.DataSource.Configuration;

public class TestDataSourceBase(DataSourceBase.Dependencies services)
    : DataSourceBase(services, "Tst.Test")
{

    // make public for testing, otherwise protected...
    public void ConfigMaskTac(string key, string mask) => this.ConfigMask(key, mask);
}