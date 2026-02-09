namespace ToSic.Eav.DataSources.PassThroughTests;

[Startup(typeof(StartupCoreDataSourcesAndTestData))]
public class PassThroughMultipleStreams(DataSourcesTstBuilder dsSvc)
{
    [Fact]
    public void Test()
    {
        var outSource = dsSvc.CreateDataSource<PassThrough>();

    }
}
