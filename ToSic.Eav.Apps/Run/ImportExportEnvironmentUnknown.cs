using System;
using System.Collections.Generic;
using ToSic.Eav.Context;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.Apps.Run
{
    public class ImportExportEnvironmentUnknown: ImportExportEnvironmentBase
    {
        public ImportExportEnvironmentUnknown(ISite site) : base(site, $"{LogNames.NotImplemented}.IExEnv") { }

        public override List<Message> TransferFilesToSite(string sourceFolder, string destinationFolder)
        {
            // don't do anything
            return new List<Message>();
        }

        public override Version TenantVersion => new Version(0,0,0);

        public override string ModuleVersion => "00.00.00";

        public override string FallbackContentTypeScope => "Default";

        public override string TemplatesRoot(int zoneId, int appId) => "/templates-root-not-defined/";

        public override void MapExistingFilesToImportSet(Dictionary<int, string> filesAndPaths, Dictionary<int, int> fileIdMap)
        {
            // don't do anything
        }

        public override void CreateFoldersAndMapToImportIds(Dictionary<int, string> foldersAndPath, Dictionary<int, int> folderIdCorrectionList, List<Message> importLog)
        {
            // don't do anything
        }
    }
}
