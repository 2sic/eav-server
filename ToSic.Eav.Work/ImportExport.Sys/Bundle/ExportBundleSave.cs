using ToSic.Eav.Apps.Sys.Paths;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Security.Files;
using ToSic.Eav.Sys;
using ToSic.Eav.WebApi.Sys.Security;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.SysFeatures;
using ToSic.Sys.Users;

namespace ToSic.Eav.WebApi.Sys.ImportExport;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ExportBundleSave(AppWorkChain<ExportBundlePrepare> exportBundlePrepare, IAppPathsMicroSvc appPathSvc, LazySvc<ISysFeaturesService> features)
    : ServiceWithSetup<IAppWorkContext>("Api.EaCtEx", connect: [exportBundlePrepare, appPathSvc, features])
{

    public bool BundleSave(IUser user, Guid exportConfiguration, int indentation)
    {
        var l = Log.Fn<bool>($"for ExportConfiguration:{exportConfiguration}");

        SecurityHelpers.ThrowIfNotSiteAdmin(user, l);

        features.Value.ThrowIfNotEnabled("This feature is required", BuiltInFeatures.DataExportImportBundles.Guid);

        var (export, fileContent) = exportBundlePrepare.New(MyOptions).CreateBundleExport(exportConfiguration, indentation);

        var appPaths = appPathSvc.Get(MyOptions.AppReader);

        var appDataPath = Path.Combine(appPaths.PhysicalPath, FolderConstants.DataFolderProtected, AppDataFoldersConstants.BundlesFolder);
        l.A($"appDataPath:'{appDataPath}'");

        try
        {
            // create App_Data\bundles unless exists
            Directory.CreateDirectory(appDataPath);

            var fileNameSafe = FileNames.SanitizeFileName(export.FileName);
            if (export.FileName != fileNameSafe) l.A($"File name sanitized:'{export.FileName}' => '{fileNameSafe}'");

            var filePath = Path.Combine(appDataPath, fileNameSafe);

            // save bundle file to App_Data\bundles
            File.WriteAllText(filePath, fileContent);
            l.A($"bundle saved, filePath:'{filePath}',size:{fileContent.Length}");

            return l.ReturnTrue();
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            return l.ReturnFalse();
        }
    }

}
