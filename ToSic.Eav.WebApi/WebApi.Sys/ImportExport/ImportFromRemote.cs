using ToSic.Eav.ImportExport.Integration;
using ToSic.Eav.ImportExport.Sys.Zip;
using ToSic.Eav.Persistence.Sys.Logging;
using ToSic.Sys.Users;

namespace ToSic.Eav.WebApi.Sys.ImportExport;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ImportFromRemote(IEnvironmentLogger envLogger, ZipFromUrlImport zipImportFromUrl, IUser user)
    : ServiceBase("Bck.Export", connect: [envLogger, zipImportFromUrl, user])
{

    public (bool, List<Message>) InstallPackage(int zoneId, int appId, bool isApp, string packageUrl)
    {
        var l = Log.Fn<(bool, List<Message>)>($"{nameof(zoneId)}:{zoneId}, {nameof(appId)}:{appId}, {nameof(isApp)}:{isApp}, url:{packageUrl}");
            
        l.A("install package:" + packageUrl);
        if (!user.IsSiteAdmin) throw new("must be admin");
        bool success;

        var importer = zipImportFromUrl;
        try
        {
            success = importer.Init(zoneId, appId, user.IsSystemAdmin)
                .ImportUrl(packageUrl, isApp);
        }
        catch (Exception ex)
        {
            envLogger.LogException(ex);
            throw new("An error occurred while installing the app: " + ex.Message, ex);
        }

        return l.Return((success, importer.Messages), success.ToString());
    }
}