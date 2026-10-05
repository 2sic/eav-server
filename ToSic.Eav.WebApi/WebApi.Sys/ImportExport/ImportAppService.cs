using ToSic.Eav.Identity;
using ToSic.Eav.ImportExport.Integration;
using ToSic.Eav.ImportExport.Sys.Zip;
using ToSic.Eav.Persistence.Sys.Logging;
using ToSic.Eav.Sys;
using ToSic.Eav.WebApi.Sys.Dto;
using ToSic.Sys.Configuration;
using ToSic.Sys.Users;

namespace ToSic.Eav.WebApi.Sys.ImportExport;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ImportAppService(
    IEnvironmentLogger envLogger,
    ZipImport zipImport,
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

        try
        {
            zipImport.Init(zoneId, null, user.IsSystemAdmin);
            var temporaryDirectory = Path.Combine(globalConfiguration.TemporaryFolder(), Guid.NewGuid().GuidCompress().Substring(0, 8));
            result.Success = zipImport.ImportZip(stream, temporaryDirectory, renameApp);
            result.Messages.AddRange(zipImport.Messages);
            return l.ReturnAsOk(result);
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            envLogger.LogException(ex);
            result.Success = false;
            result.Messages.AddRange(zipImport.Messages);
            return l.ReturnAsError(result);
        }
    }
}
