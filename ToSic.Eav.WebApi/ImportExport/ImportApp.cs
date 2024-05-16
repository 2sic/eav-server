using System.IO;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Context;
using ToSic.Eav.Identity;
using ToSic.Eav.ImportExport.Internal;
using ToSic.Eav.ImportExport.Internal.ImportHelpers;
using ToSic.Eav.ImportExport.Internal.Zip;
using ToSic.Eav.Integration.Environment;
using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Persistence.Logging;
using ISite = ToSic.Eav.Context.ISite;

namespace ToSic.Eav.WebApi.ImportExport;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ImportApp(
    IEnvironmentLogger envLogger,
    ZipImport import,
    IGlobalConfiguration globalConfiguration,
    IUser user,
    AppFinder appFinder,
    ISite site,
    Generator<XmlImportWithFiles> xmlImpExpFiles,
    IEavFeaturesService features)
    : ServiceBase("Bck.Export",
        connect: [envLogger, import, globalConfiguration, user, appFinder, site, xmlImpExpFiles, features])
{
    public ImportResultDto Import(Stream stream, int zoneId, string renameApp)
    {
        Log.A("start import app from stream");
        var result = new ImportResultDto();

        if (!string.IsNullOrEmpty(renameApp)) Log.A($"new app name: {renameApp}");

        var zipImport = import;
        try
        {
            zipImport.Init(zoneId, null, user.IsSystemAdmin);
            var temporaryDirectory = Path.Combine(globalConfiguration.TemporaryFolder, Mapper.GuidCompress(Guid.NewGuid()).Substring(0, 8));

            // Increase script timeout to prevent timeouts
            result.Success = zipImport.ImportZip(stream, temporaryDirectory, renameApp);
            result.Messages.AddRange(zipImport.Messages);
        }
        catch (Exception ex)
        {
            envLogger.LogException(ex);
            result.Success = false;
            result.Messages.AddRange(zipImport.Messages);
        }
        return result;
    }

    public ImportResultDto Import(string zipPath, int zoneId, string renameApp, int? inheritAppId = null)
    {
        Log.A($"start import app from:{zipPath} with inheritAppId:{inheritAppId}");
        var result = new ImportResultDto();

        if (!string.IsNullOrEmpty(renameApp)) Log.A($"new app name: {renameApp}");

        var zipImport = import;
        try
        {
            zipImport.Init(zoneId, null, user.IsSystemAdmin);
            var temporaryDirectory = Path.Combine(globalConfiguration.TemporaryFolder, Mapper.GuidCompress(Guid.NewGuid()).Substring(0, 8));

            result.Success = zipImport.ImportZip(zipPath, temporaryDirectory, renameApp, inheritAppId);
            result.Messages.AddRange(zipImport.Messages);
        }
        catch (Exception ex)
        {
            envLogger.LogException(ex);
            result.Success = false;
            result.Messages.AddRange(zipImport.Messages);
        }
        return result;
    }

    /// <summary>
    /// Get list of pending apps.
    /// List all app folders in the 2sxc which:
    /// - are not installed as apps yet
    /// - have a App_Data/app.xml
    /// </summary>
    /// <param name="zoneId"></param>
    /// <returns></returns>
    public IEnumerable<PendingAppDto> GetPendingApps(int zoneId)
    {
        var l = Log.Fn<IEnumerable<PendingAppDto>>($"list all app folders for zoneId.{zoneId}");
        var result = new List<PendingAppDto>();

        // loop through each app folder and find pending apps
        foreach (var directoryPath in Directory.GetDirectories(site.AppsRootPhysicalFull))
        {
            Log.A($"find pending app in folder:{directoryPath}");
                
            var folderName = Path.GetFileName(directoryPath);

            // skip folder when app is already installed
            if (appFinder.AppIdFromFolderName(zoneId, folderName) != AppConstants.AppIdNotFound)
            {
                Log.A($"skip, app is already installed");
                continue;
            }

            // skip folder when App_Data/app.xml is missing
            var appXml = Path.Combine(directoryPath, Constants.AppDataProtectedFolder, Constants.AppDataFile);
            if (!File.Exists(appXml))
            {
                Log.A($"skip, App_Data/app.xml is missing");
                continue;
            }

            try
            {
                var importer = xmlImpExpFiles.New().Init(null, false);
                var importXmlReader = new ImportXmlReader(appXml, importer, Log);
                var pendingAppDto = new PendingAppDto
                {
                    ServerFolder = folderName,
                    Name = importXmlReader.DisplayName,
                    Description = importXmlReader.Description,
                    Version = importXmlReader.Version,
                    Folder = importXmlReader.AppFolder
                };
                result.Add(pendingAppDto);
                Log.A($"pending app {pendingAppDto.Name}, v{pendingAppDto.Version}");
            }
            catch (Exception e)
            {
                Log.Ex(e);
            }
        }

        return l.ReturnAsOk(result);
    }

    /// <summary>
    /// Install pending apps
    /// </summary>
    /// <param name="zoneId"></param>
    /// <param name="pendingApps"></param>
    /// <returns></returns>
    public ImportResultDto InstallPendingApps(int zoneId, IEnumerable<PendingAppDto> pendingApps)
    {
        Log.A($"Install pending apps start");
        var result = new ImportResultDto();

        // before installation, ensure that feature is enabled
        if (!features.IsEnabled(BuiltInFeatures.AppSyncWithSiteFiles))
        {
            var message = $"Skip all. Can't install pending apps because feature {BuiltInFeatures.AppSyncWithSiteFiles.NameId} is not enabled.";
            var messages = new List<Message>() { new(message, Message.MessageTypes.Warning)};
            Log.A(message);
            result.Success = false;
            result.Messages.AddRange(messages);
            return result;
        }

        try
        {
            import.Init(zoneId, null, user.IsSystemAdmin);
            foreach (var pendingAppDto in pendingApps)
            {
                var appDirectory = Path.Combine(site.AppsRootPhysicalFull, pendingAppDto.ServerFolder);
                var importMessage = new List<Message>();
                // do we need to rename pending app
                var rename = pendingAppDto.ServerFolder.Equals(pendingAppDto.Folder, StringComparison.InvariantCultureIgnoreCase) ? string.Empty : pendingAppDto.ServerFolder;
                // Increase script timeout to prevent timeouts
                result.Success = import.ImportApp(rename, appDirectory, importMessage, pendingApp: true);
                result.Messages.AddRange(importMessage);
            }
        }
        catch (Exception ex)
        {
            envLogger.LogException(ex);
            result.Success = false;
            result.Messages.AddRange(import.Messages);
        }
        return result;
    }
}