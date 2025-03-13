using ToSic.Eav.Repositories;

namespace ToSic.Eav.ImportExport.Tests19.Persistence.File;

/// <summary>
/// This is an additional GlobalFolderRepository.
/// It will be picked up by reflection and also loaded.
/// </summary>
public class AdditionalGlobalFolderRepositoryForReflection: FolderBasedRepository
{
    // this will be set from externally in various tests
    public static string PathToUse = "";

    public override List<string> RootPaths => [PathToUse];
}