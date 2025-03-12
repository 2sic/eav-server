using ToSic.Eav.Repositories;

namespace ToSic.Eav.ImportExport.Tests19.Persistence.File.RuntimeLoader;

public class TestGlobalFolderRepository: FolderBasedRepository
{
    // this will be set from externally in various tests
    public static string PathToUse = "";

    public override List<string> RootPaths => [PathToUse];
}