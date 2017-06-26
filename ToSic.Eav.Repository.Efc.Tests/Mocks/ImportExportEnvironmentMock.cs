using System;
using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Interfaces;
using ExportImportMessage = ToSic.Eav.Persistence.Logging.ExportImportMessage;

namespace ToSic.Eav.Repository.Efc.Tests.Mocks
{
    internal class ImportExportEnvironmentMock : IImportExportEnvironment
    {
        public List<ExportImportMessage> Messages { get; } = new List<ExportImportMessage>();

        public string BasePath { get; set; }= @"C:\Projects\eav-server\ToSic.Eav.Repository.Efc.Tests\";


        public virtual void TransferFilesToTennant(string sourceFolder, string destinationFolder)
        {
        }

        public virtual Version TennantVersion => new Version(8, 0);

        public virtual string ModuleVersion => "08.12.00";

        public virtual string FallbackContentTypeScope => "2SexyContent";

        public string DefaultLanguage => "en-US";

        public string TemplatesRoot(int zoneId, int appId) => BasePath + @"Destination\" + appId + @"Templates";

        public string TargetPath(string folder) => BasePath + @"Destination\" + folder;

        public void MapExistingFilesToImportSet(Dictionary<int, string> filesAndPaths, Dictionary<int, int> fileIdMap)
        {
        }

        public void CreateFoldersAndMapToImportIds(Dictionary<int, string> foldersAndPath, Dictionary<int, int> folderIdCorrectionList, List<ExportImportMessage> importLog)
        {
        }
       
        public SaveOptions SaveOptions(int zoneId) => new SaveOptions(DefaultLanguage, new ZoneRuntime(zoneId).Languages(true));
    }
}
