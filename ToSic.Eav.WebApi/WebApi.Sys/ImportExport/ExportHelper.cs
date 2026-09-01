using ToSic.Eav.ImportExport.Sys.Zip;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.SysFeatures;

namespace ToSic.Eav.WebApi.Sys.ImportExport;

public class ExportHelper(
    Generator<ImpExpHelpers> impExpHelpers,
    Generator<ZipExport, ZipExport.Options> exportGenerator
) : ServiceBase("Sxc.ImExHl", connect: [impExpHelpers, exportGenerator])
{

    internal (IAppReader appReader, ZipExport zipExport) GetZipExportAndCheckZoneSwitchPermissions(IAppIdentity appIdentity)
    {
        var (appReader, appPaths) = impExpHelpers.New().GetReaderAndPathsAfterZoneSwitchPermissionCheck(appIdentity);
        var zipExport = exportGenerator.New(new()
        {
            ZoneId = appIdentity.ZoneId,
            AppId = appIdentity.AppId,
            AppFolder = appReader.Specs.Folder,
            PhysicalAppPath = appPaths.PhysicalPath,
            PhysicalPathGlobal = appPaths.PhysicalPathShared
        });
        return (appReader, zipExport);
    }

    internal static void SyncWithSiteFilesVerifyFeaturesOrThrow(ISysFeaturesService features, bool withSiteFiles)
    {
        if (!withSiteFiles)
            return;
        features.ThrowIfNotEnabled("Requires features enabled to sync with site files ",
            BuiltInFeatures.AppSyncWithSiteFiles.Guid);
    }

}