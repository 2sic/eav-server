using System.Diagnostics;
using ToSic.Eav.Data.Source;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Repositories;
using Xunit.Abstractions;

namespace ToSic.Eav.ImportExport.Tests.Persistence.File;

internal class LoaderHelper(string scenarioDeep, ILog parentLog): HelperBase(parentLog, "Ldr.Helper")
{
    internal IList<IEntity> LoadAllQueryEntities(FileSystemLoader loaderRaw, ITestOutputHelper output)
    {
        var testStorageRoot = TestFiles.GetTestPath(scenarioDeep);
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
        return [];
    }

    internal IList<IContentType> LoadAllTypes(FileSystemLoader loaderRaw, ILog log) //, string? subfolderOnly = null)
    {
        // Log the root for debugging in case files are missing
        var testRoot = TestFiles.GetTestPath(scenarioDeep); // PersistenceTestConstants.TestStorageRoot(TestContext);
        //if (subfolderOnly.HasValue())
        //    testRoot += subfolderOnly + '\\';
        Trace.WriteLine("Test folder: " + testRoot);

        var loader = loaderRaw
            .Init(Constants.PresetAppId, testRoot, RepositoryTypes.TestingDoNotUse, /*subfolderOnly != null*/true, null);
        IList<IContentType> cts;
        try
        {
            cts = loader.ContentTypes();
        }
        finally
        {
            Trace.Write(Log.Dump());
        }
        return cts;
    }
}