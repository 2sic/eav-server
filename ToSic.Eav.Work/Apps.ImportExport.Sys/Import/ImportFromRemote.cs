using ToSic.Eav.ImportExport.Integration;
using ToSic.Eav.ImportExport.Sys.Zip;
using ToSic.Sys.Users;

namespace ToSic.Eav.Apps.ImportExport.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ImportFromRemote(IEnvironmentLogger envLogger, Generator<ZipFromUrlImport, ZipImport.Options> zipImportFromUrl, IUser user)
    : ServiceBase("Bck.Export", connect: [envLogger, zipImportFromUrl, user])
{

    public ZipImport.Result InstallPackage(int zoneId, int appId, bool isApp, string packageUrl, string? newName = null)
    {
        var l = Log.Fn<ZipImport.Result>($"{nameof(zoneId)}:{zoneId}, {nameof(appId)}:{appId}, {nameof(isApp)}:{isApp}, url:'{packageUrl}', {nameof(newName)}:'{newName}'");
            
        l.A("install package:" + packageUrl);
        if (!user.IsSiteAdmin)
            throw new("must be admin");
        ZipImport.Result result;

        
        var importer = zipImportFromUrl.New(new(zoneId, appId, AllowCode: true));
        try
        {
            result = importer.ImportUrl(packageUrl, isApp, newName);
        }
        catch (Exception ex)
        {
            envLogger.LogException(ex);
            throw new($"An error occurred while installing the app: {ex.Message}", ex);
        }

        return l.Return(result, result.ToString());
    }
}