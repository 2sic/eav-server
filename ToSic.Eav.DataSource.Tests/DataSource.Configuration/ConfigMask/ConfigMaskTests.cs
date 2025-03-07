namespace ToSic.Eav.DataSource.Configuration;

[Startup(typeof(TestStartupEavCoreAndDataSources))]
public class ConfigMaskTests(DataSourcesTstBuilder dsSvc)
{
    [Fact]
    public void ConfigMaskClassic()
    {
        var ds = GetDs();
        ds.ConfigMask("Something", $"[{DataSourceConstants.MyConfigurationSourceName}:Test]");
        var ccc = ds.CacheRelevantConfigurations.FirstOrDefault();
        Equal("Something", ccc);

        var pair = ds.Configuration.Values.FirstOrDefault();
        Equal("Something", pair.Key);
        Equal($"[{DataSourceConstants.MyConfigurationSourceName}:Test]", pair.Value);
    }



    private TestDataSourceBase GetDs() => dsSvc.CreateDataSource<TestDataSourceBase>();

}