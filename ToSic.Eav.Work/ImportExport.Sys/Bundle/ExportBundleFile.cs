using ToSic.Eav.WebApi.Sys.Security;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.SysFeatures;
using ToSic.Sys.Users;

namespace ToSic.Eav.ImportExport.Sys.Bundle;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ExportBundleFile(AppWorkChain<ExportBundlePrepare> exportBundlePrepare, LazySvc<ISysFeaturesService> features)
    : ServiceWithSetup<IAppWorkContext>("Api.EaCtEx", connect: [exportBundlePrepare, features])
{

    public FileResult JsonBundleExport(IUser user, Guid exportConfiguration, int indentation)
    {
        var l = Log.Fn<FileResult>($"create Json Bundle Export for ExportConfiguration:{exportConfiguration}");
        SecurityHelpers.ThrowIfNotSiteAdmin(user, l);

        features.Value.ThrowIfNotEnabled("This feature is required", BuiltInFeatures.DataExportImportBundles.Guid);

        var (export, fileContent) = exportBundlePrepare.New(MyOptions).CreateBundleExport(exportConfiguration, indentation);

        return l.ReturnAsOk(new()
        {
            FileName = export.FileName,
            ContentType = MimeTypeConstants.Json,
            FileBytes = System.Text.Encoding.UTF8.GetBytes(fileContent)
        });
    }
}
