using System;
using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Internal.Environment;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Repository.Efc.Tests.Mocks
{
    internal class ImportExportEnvironmentMock(IAppStates appStates)
        : ServiceBase("Mck.ImpExp", connect: [appStates]), IImportExportEnvironment
    {
        public string BasePath { get; set; }= @"C:\Projects\2sxc\eav-server\ToSic.Eav.Repository.Efc.Tests\";


        public virtual List<Message> TransferFilesToSite(string sourceFolder, string destinationFolder)
        {
            return new();
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
       
        public SaveOptions SaveOptions(int zoneId) => new(DefaultLanguage, appStates.Languages(zoneId, true)/* new ZoneRuntime().Init(zoneId, Log).Languages(true)*/);
    }
}
