using ToSic.Eav.Data.Source;
using ToSic.Eav.ImportExport.Tests19.Persistence.File.RuntimeLoader;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Repositories;
using Xunit.Abstractions;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.ImportExport.Tests19.Persistence.File;

public class CountQueriesInScenarioMini(ITestOutputHelper output, FileSystemLoader loaderRaw) : ServiceBase("test"), IClassFixture<DoFixtureStartup<ScenarioMini>>
{
    [Fact]
    public void FLoader_LoadQueriesAndCount()
    {
        var cts = LoadAllQueryEntities();
        Equal(ScenarioMini.Has3Queries, cts.Count);//, "test case has 3 content-types to deserialize");
    }

    private IList<IEntity> LoadAllQueryEntities()
    {
        var testStorageRoot = TestFiles.GetTestPath($"{PersistenceTestConstants.ScenarioRoot}{PersistenceTestConstants.PathMini}\\App_Data\\system");
        output.WriteLine($"path:'{testStorageRoot}'");
        var loader = loaderRaw.Init(Constants.PresetAppId, testStorageRoot, RepositoryTypes.TestingDoNotUse, false, null);
        
        try
        {
            return DirectEntitiesSource.Using(set => loader.Entities(FsDataConstants.QueriesFolder, 0, set.Source));
        }
        finally
        {
            output.WriteLine(Log.Dump());
        }
        return []; }
}