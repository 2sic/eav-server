using ToSic.Eav.Context;
using ToSic.Eav.ImportExport.Integration;
using ToSic.Eav.ImportExport.Sys.Zip;
using ToSic.Eav.Persistence.Sys.Logging;
using ToSic.Eav.WebApi.Sys.Dto;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Users;

namespace ToSic.Eav.WebApi.Sys.ImportExport;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class PendingAppInstall(
    IEnvironmentLogger envLogger,
    ZipImport zipImport,
    IUser user,
    ISite site,
    ISysFeaturesService features)
    : ServiceBase("Bck.Export", connect: [envLogger, zipImport, user, site, features])
{
    public ImportResultDto InstallPendingApps(int zoneId, IEnumerable<PendingAppDto> pendingApps)
    {
        var l = Log.Fn<ImportResultDto>("Install pending apps start");
        var result = new ImportResultDto();

        // If feature disabled, exit early with message
        if (!features.IsEnabled(BuiltInFeatures.AppSyncWithSiteFiles))
        {
            var message = $"Skip all. Can't install pending apps because feature {BuiltInFeatures.AppSyncWithSiteFiles.NameId} is not enabled.";
            l.A(message);
            result.Success = false;
            result.Messages.Add(new(message, Message.MessageTypes.Warning));
            return l.ReturnAsOk(result);
        }

        try
        {
            zipImport.Init(zoneId, null, user.IsSystemAdmin);
            foreach (var pendingAppDto in pendingApps)
            {
                var appDirectory = Path.Combine(site.AppsRootPhysicalFull, pendingAppDto.ServerFolder);
                // careful: the import messages is actually modified in a deeper call
                // should be improved to have a clear flow returning the messages instead of modifying a list in a deeper call
                var importMessage = new List<Message>();
                var rename = pendingAppDto.ServerFolder.EqualsInsensitive(pendingAppDto.Folder)
                    ? string.Empty
                    : pendingAppDto.ServerFolder;
                result.Success = zipImport.ImportApp(rename, appDirectory, importMessage, pendingApp: true);
                result.Messages.AddRange(importMessage);
            }
            return l.ReturnAsOk(result);
        }
        catch (Exception ex)
        {
            envLogger.LogException(ex);
            result.Success = false;
            result.Messages.AddRange(zipImport.Messages);
            return l.ReturnAsError(result);
        }
    }
}
