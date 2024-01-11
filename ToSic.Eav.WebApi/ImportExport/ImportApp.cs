using System;
using System.Collections.Generic;
using System.IO;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Apps.ImportExport.ImportHelpers;
using ToSic.Eav.Context;
using ToSic.Eav.Identity;
using ToSic.Eav.Integration.Environment;
using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Internal.Features;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.WebApi.Dto;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using ISite = ToSic.Eav.Context.ISite;

namespace ToSic.Eav.WebApi.ImportExport;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ImportApp: ServiceBase
{
    #region DI Constructor

    public ImportApp(IEnvironmentLogger envLogger, ZipImport zipImport, IGlobalConfiguration globalConfiguration, IUser user, AppFinder appFinder, ISite site, Generator<XmlImportWithFiles> xmlImpExpFiles, IEavFeaturesService features) : base("Bck.Export")
    {
        ConnectServices(
            _envLogger = envLogger,
            _zipImport = zipImport,
            _globalConfiguration = globalConfiguration,
            _user = user,
            _appFinder = appFinder,
            _site = site,
            _xmlImpExpFiles = xmlImpExpFiles,
            _features = features
        );
    }

    private readonly IEnvironmentLogger _envLogger;
    private readonly ZipImport _zipImport;
    private readonly IGlobalConfiguration _globalConfiguration;
    private readonly IUser _user;
    private readonly AppFinder _appFinder;
    private readonly ISite _site;
    private readonly Generator<XmlImportWithFiles> _xmlImpExpFiles;
    private readonly IEavFeaturesService _features;

    #endregion

    public ImportResultDto Import(Stream stream, int zoneId, string renameApp)
    {
        Log.A("import app start");
        var result = new ImportResultDto();

        if (!string.IsNullOrEmpty(renameApp)) Log.A($"new app name: {renameApp}");

        var zipImport = _zipImport;
        try
        {
            zipImport.Init(zoneId, null, _user.IsSystemAdmin);
            var temporaryDirectory = Path.Combine(_globalConfiguration.TemporaryFolder, Mapper.GuidCompress(Guid.NewGuid()).Substring(0, 8));

            // Increase script timeout to prevent timeouts
            result.Success = zipImport.ImportZip(stream, temporaryDirectory, renameApp);
            result.Messages.AddRange(zipImport.Messages);
        }
        catch (Exception ex)
        {
            _envLogger.LogException(ex);
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
        var wrapLog = Log.Fn<IEnumerable<PendingAppDto>>($"list all app folders for zoneId.{zoneId}");
        var result = new List<PendingAppDto>();

        // loop through each app folder and find pending apps
        foreach (var directoryPath in Directory.GetDirectories(_site.AppsRootPhysicalFull))
        {
            Log.A($"find pending app in folder:{directoryPath}");
                
            var folderName = Path.GetFileName(directoryPath);

            // skip folder when app is already installed
            if (_appFinder.AppIdFromFolderName(zoneId, folderName) != AppConstants.AppIdNotFound)
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
                var importer = _xmlImpExpFiles.New().Init(null, false);
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

        return wrapLog.ReturnAsOk(result);
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
        if (!_features.IsEnabled(BuiltInFeatures.AppSyncWithSiteFiles))
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
            _zipImport.Init(zoneId, null, _user.IsSystemAdmin);
            foreach (var pendingAppDto in pendingApps)
            {
                var appDirectory = Path.Combine(_site.AppsRootPhysicalFull, pendingAppDto.ServerFolder);
                var importMessage = new List<Message>();
                // do we need to rename pending app
                var rename = pendingAppDto.ServerFolder.Equals(pendingAppDto.Folder, StringComparison.InvariantCultureIgnoreCase) ? string.Empty : pendingAppDto.ServerFolder;
                // Increase script timeout to prevent timeouts
                result.Success = _zipImport.ImportApp(rename, appDirectory, importMessage, pendingApp: true);
                result.Messages.AddRange(importMessage);
            }
        }
        catch (Exception ex)
        {
            _envLogger.LogException(ex);
            result.Success = false;
            result.Messages.AddRange(_zipImport.Messages);
        }
        return result;
    }
}