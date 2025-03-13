using ToSic.Eav.Data.Source;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Repositories;
using Xunit.Abstractions;

namespace ToSic.Eav.ImportExport.Tests19.Persistence.File;

internal static class LoadQueryEntitiesHelper
{
    internal static IList<IEntity> LoadAllQueryEntities(FileSystemLoader loaderRaw, ITestOutputHelper output, ILog log)
    {
        var testStorageRoot = TestFiles.GetTestPath(PersistenceTestConstants.ScenarioMiniDeep);
        output.WriteLine($"path:'{testStorageRoot}'");
        var loader = loaderRaw.Init(Constants.PresetAppId, testStorageRoot, RepositoryTypes.TestingDoNotUse, false, null);
        
        try
        {
            return DirectEntitiesSource.Using(set => loader.Entities(FsDataConstants.QueriesFolder, 0, set.Source));
        }
        finally
        {
            output.WriteLine(log.Dump());
        }
        return [];
    }
}