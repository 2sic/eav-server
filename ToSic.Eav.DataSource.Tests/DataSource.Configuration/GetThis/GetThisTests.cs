namespace ToSic.Eav.DataSource.Configuration;

[Startup(typeof(TestStartupEavCoreAndDataSources))]
public class GetThisTests(DataSourcesTstBuilder dsSvc)
{

    [Fact] public void GetThisString() => Equal(TestDsGetThis.ExpectedGetThisString, GetPropsDs().GetThisString);

    [Fact] public void GetThisTrue() => True(GetPropsDs().GetThisTrue);

    [Fact] public void GetThisFalseDefault() => False(GetPropsDs().GetThisFalseDefault);

    [Fact] public void GetThisFalseInitialized() => False(GetPropsDs().GetThisFalseInitialized);


    private TestDsGetThis GetPropsDs()
    {
        var ds = dsSvc.CreateDataSource<TestDsGetThis>();
        ds.Configuration.Parse();
        return ds;
    }
}