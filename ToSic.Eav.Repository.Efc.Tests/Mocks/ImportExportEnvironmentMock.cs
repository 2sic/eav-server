using System;
using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.Repository.Efc.Tests.Mocks
{
    internal class ImportExportEnvironmentMock : HasLog, IImportExportEnvironment
    {
        #region Constructors

        public ImportExportEnvironmentMock(IAppStates appStates) : base("Mck.ImpExp")
        {
            _appStates = appStates;
        }
        private readonly IAppStates _appStates;

        public IImportExportEnvironment Init(ILog parent)
        {
            Log.LinkTo(parent);
            return this;
        }


        #endregion

        public string BasePath { get; set; }= @"C:\Projects\2sxc\eav-server\ToSic.Eav.Repository.Efc.Tests\";


        public virtual List<Message> TransferFilesToSite(string sourceFolder, string destinationFolder)
        {
            return new List<Message>();
        }

        public virtual Version TenantVersion => new Version(8, 0);

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
       
        public SaveOptions SaveOptions(int zoneId) => new SaveOptions(DefaultLanguage, _appStates.Languages(zoneId, true)/* new ZoneRuntime().Init(zoneId, Log).Languages(true)*/);
    }
}
