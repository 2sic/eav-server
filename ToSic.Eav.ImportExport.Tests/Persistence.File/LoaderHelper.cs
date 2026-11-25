using System.Diagnostics;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Entities.Sources;
using Xunit.Abstractions;

namespace ToSic.Eav.Persistence.File;

internal class LoaderHelper(string scenarioDeep, ILog parentLog): HelperBase(parentLog, "Ldr.Helper")
{
    internal IList<IEntity> LoadAllQueryEntities(Generator<FileSystemLoader, FileSystemLoaderOptions> loaderRaw, ITestOutputHelper output)
    {
        var testStorageRoot = TestFiles.GetTestPath(scenarioDeep);
        output.WriteLine($"path:'{testStorageRoot}'");
        var loader = loaderRaw.New(new()
        {
            JsonReaderOptions = FileSystemLoaderOptions.CreateJsonOptionsDyn(KnownAppsConstants.PresetAppId, log: Log),
            //AppId = KnownAppsConstants.PresetAppId,
            Path = testStorageRoot,
            RepoType = RepositoryTypes.TestingDoNotUse,
            IgnoreMissing = false,
        });
        
        try
        {
            return DirectEntitiesSource.Using(set => loader.Entities(AppDataFoldersConstants.QueriesFolder, set.Source));
        }
        finally
        {
            output.WriteLine(Log.Dump());
        }
    }

    internal ICollection<IContentType> LoadAllTypes(FileSystemLoader loaderRaw, ILog log)
    {
        // Log the root for debugging in case files are missing
        var testRoot = TestFiles.GetTestPath(scenarioDeep);
        Trace.WriteLine("Test folder: " + testRoot);

        var loader = loaderRaw;
        loader.Setup(new()
        {
            JsonReaderOptions = FileSystemLoaderOptions.CreateJsonOptionsDyn(KnownAppsConstants.PresetAppId, log: log),
            //AppId = KnownAppsConstants.PresetAppId,
            Path = testRoot,
            RepoType = RepositoryTypes.TestingDoNotUse,
            IgnoreMissing = true,
        });
        ICollection<IContentType> cts;
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