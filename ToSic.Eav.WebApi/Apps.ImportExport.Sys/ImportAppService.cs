using ToSic.Eav.Identity;
using ToSic.Eav.ImportExport.Integration;
using ToSic.Eav.ImportExport.Sys.Zip;
using ToSic.Eav.WebApi.Sys.Dto;
using ToSic.Sys.Configuration;
using ToSic.Sys.Users;

namespace ToSic.Eav.Apps.ImportExport.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ImportAppService(
    IEnvironmentLogger envLogger,
    Generator<ZipImport, ZipImport.Options> zipImport,
    IGlobalConfiguration globalConfiguration,
    IUser user)
    : ServiceBase("Bck.Export", connect: [envLogger, zipImport, globalConfiguration, user])
{
    public ImportResultDto Import(Stream stream, int zoneId, string renameApp)
    {
        var l = Log.Fn<ImportResultDto>("start import app from stream");
        var result = new ImportResultDto();

        if (!string.IsNullOrEmpty(renameApp))
            l.A($"new app name: {renameApp}");

        var zipImportInstance = zipImport.New(new(zoneId, null, user.IsSystemAdmin));
        try
        {
            var temporaryDirectory = Path.Combine(globalConfiguration.TemporaryFolder(), Guid.NewGuid().GuidCompress().Substring(0, 8));
            var importResult = zipImportInstance.ImportZip(stream, temporaryDirectory, renameApp);
            result.Success = importResult.Success;
            result.Messages.AddRange(importResult.Messages);
            return l.ReturnAsOk(result);
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            envLogger.LogException(ex);
            result.Success = false;
            result.Messages.AddRange(zipImportInstance.Messages);
            return l.ReturnAsError(result);
        }
    }
}
