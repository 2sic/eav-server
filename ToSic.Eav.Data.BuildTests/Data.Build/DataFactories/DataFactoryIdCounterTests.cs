namespace ToSic.Eav.Data.Build.DataFactories;

[Startup(typeof(StartupTestsEavDataBuild))]
public class DataFactoryIdCounterTests(IDataFactory dataFactory)
{
    [Fact]
    public void DefaultFactoryInitialHasIdSeed1()
        => Equal(1, dataFactory.IdCounter);

}
