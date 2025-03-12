using ToSic.Eav.Data.Source;
using ToSic.Eav.ImportExport.Tests19.Persistence.File.RuntimeLoader;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Repositories;
using Xunit.Abstractions;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.ImportExport.Tests19.Persistence.File;

public class Tst_FileLoader_Query(ITestOutputHelper output, FileSystemLoader loaderRaw) : ServiceBase("test"), IClassFixture<DoFixtureStartup<ScenarioDotData>>
{
    [Fact]
    public void FLoader_LoadQueriesAndCount()
    {
        var cts = LoadAllQueryEntities();
        Equal(3, cts.Count);//, "test case has 3 content-types to deserialize");
    }

    private IList<IEntity> LoadAllQueryEntities()
    {
        var testStorageRoot = TestFiles.GetTestPath($"{PersistenceTestConstants.ScenarioRoot}{PersistenceTestConstants.PathWith3Types}\\App_Data\\system");
        output.WriteLine($"path:'{testStorageRoot}'");
        var loader = loaderRaw.Init(Constants.PresetAppId, testStorageRoot, RepositoryTypes.TestingDoNotUse, false, null);
        IList<IEntity> entities;
        try
        {
            entities = DirectEntitiesSource.Using(set => loader.Entities(FsDataConstants.QueriesFolder, 0, set.Source));
        }
        finally
        {
            output.WriteLine(Log.Dump());
        }
        return entities;
    }
}