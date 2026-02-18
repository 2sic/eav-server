namespace ToSic.Eav.Data.Build.DataFactories;

[Startup(typeof(StartupTestsEavDataBuild))]
public class DataFactoryOptionTests(IDataFactory dataFactory)
{
    [Fact]
    public void DefaultFactoryInitialHasIdSeed1()
        => Equal(1, dataFactory.IdCounter);

    [Fact]
    public void DefaultFactoryNewHasIdSeed1()
        => Equal(1, dataFactory.SpawnNew(new()).IdCounter);

    [Fact]
    public void DefaultFactoryNewWithOptionsHasIdSeedAsSet()
        => Equal(999, dataFactory.SpawnNew(new() { IdSeed = 999 }).IdCounter);
}