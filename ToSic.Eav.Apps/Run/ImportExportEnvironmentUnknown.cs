using System;
using System.Collections.Generic;
using ToSic.Eav.Context;
using ToSic.Eav.Internal.Unknown;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Run;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Run;

public class ImportExportEnvironmentUnknown: ImportExportEnvironmentBase, IIsUnknown
{
    public ImportExportEnvironmentUnknown(ISite site, IAppStates appStates, WarnUseOfUnknown<ImportExportEnvironmentUnknown> _) 
        : base(site, appStates, $"{LogScopes.NotImplemented}.IExEnv") { }

    public override List<Message> TransferFilesToSite(string sourceFolder, string destinationFolder)
    {
        // don't do anything
        return new List<Message>();
    }

    public override Version TenantVersion => new(0,0,0);

    public override string FallbackContentTypeScope => "Default";

    public override string TemplatesRoot(int zoneId, int appId) => "/templates-root-not-defined/";

    public override string GlobalTemplatesRoot(int zoneId, int appId) => "/global-templates-root-not-defined/";

    public override void MapExistingFilesToImportSet(Dictionary<int, string> filesAndPaths, Dictionary<int, int> fileIdMap)
    {
        // don't do anything
    }

    public override void CreateFoldersAndMapToImportIds(Dictionary<int, string> foldersAndPath, Dictionary<int, int> folderIdCorrectionList, List<Message> importLog)
    {
        // don't do anything
    }
}