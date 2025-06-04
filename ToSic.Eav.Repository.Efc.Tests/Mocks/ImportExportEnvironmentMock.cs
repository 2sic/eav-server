using ToSic.Eav.Apps;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.ImportExport.Integration;
using ToSic.Eav.Persistence.Sys.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Repository.Efc.Tests.Mocks;

public class ImportExportEnvironmentMock(IAppsCatalog appsCatalog)
    : ServiceBase("Mck.ImpExp", connect: [appsCatalog]), IImportExportEnvironment
{
    // This should point to a subfolder in the bin, so that temp data is created there
    private string BasePath => TestFiles.GetTestPath("") + "\\";


    public virtual List<Message> TransferFilesToSite(string sourceFolder, string destinationFolder)
    {
        return [];
    }

    public virtual Version TenantVersion => new(8, 0);

    public virtual string ModuleVersion => "08.12.00";

    public virtual string FallbackContentTypeScope => "2SexyContent";

    public string DefaultLanguage => "en-US";

    public string TemplatesRoot(int zoneId, int appId) => BasePath + @"Destination\" + appId + @"Views";

    public string GlobalTemplatesRoot(int zoneId, int appId) => BasePath + @"DestinationGlobal\" + appId + @"Views";

    public string TargetPath(string folder) => BasePath + @"Destination\" + folder;


    public void MapExistingFilesToImportSet(Dictionary<int, string> filesAndPaths, Dictionary<int, int> fileIdMap)
    {
    }

    public void CreateFoldersAndMapToImportIds(Dictionary<int, string> foldersAndPath, Dictionary<int, int> folderIdCorrectionList, List<Message> importLog)
    {
    }
       
    public SaveOptions SaveOptions(int zoneId) => new()
    {
        PrimaryLanguage = DefaultLanguage,
        Languages = appsCatalog.Zone(zoneId).Languages,
    };
}