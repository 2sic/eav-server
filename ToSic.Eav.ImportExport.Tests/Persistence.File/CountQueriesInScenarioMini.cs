using ToSic.Eav.Persistence.File;
using Xunit.Abstractions;

namespace ToSic.Eav.ImportExport.Tests.Persistence.File;

public class CountQueriesInScenarioMini(ITestOutputHelper output, FileSystemLoader loaderRaw) : ServiceBase("test"), IClassFixture<DoFixtureStartup<ScenarioMini>>
{
    [Fact]
    public void FLoader_LoadQueriesAndCount()
    {
        var cts = new LoaderHelper(PersistenceTestConstants.ScenarioMiniDeep, Log)
            .LoadAllQueryEntities(loaderRaw, output);
        Equal(ScenarioMini.Has3Queries, cts.Count);//, "test case has 3 content-types to deserialize");
    }
}