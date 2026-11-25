using ToSic.Eav.Persistence.Sys.Logging;

#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.ImportExport.Integration;

internal class ImportExportEnvironmentUnknown(ImportExportPathsAndOptions pathAndOptions, WarnUseOfUnknown<ImportExportEnvironmentUnknown> _)
    : ServiceBase($"{LogScopes.NotImplemented}.IExEnv"), IImportExportEnvironment, IIsUnknown
{
    public List<Message> TransferFilesToSite(string sourceFolder, string destinationFolder)
    {
        // don't do anything
        return [];
    }

    public ImportExportPathsAndOptions PathsAndOptions => pathAndOptions;

    public Version TenantVersion => new(0,0,0);

    public void MapExistingFilesToImportSet(Dictionary<int, string> filesAndPaths, Dictionary<int, int> fileIdMap)
    {
        // don't do anything
    }

    public void CreateFoldersAndMapToImportIds(Dictionary<int, string> foldersAndPath, Dictionary<int, int> folderIdCorrectionList, List<Message> importLog)
    {
        // don't do anything
    }
}