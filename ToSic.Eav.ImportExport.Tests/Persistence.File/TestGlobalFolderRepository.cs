using System.Collections.Generic;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.ImportExport.Tests.Persistence.File;

public class TestGlobalFolderRepository: FolderBasedRepository
{
    // this will be set from externally in various tests
    public static string PathToUse = "";

    public override List<string> RootPaths => [PathToUse];
}