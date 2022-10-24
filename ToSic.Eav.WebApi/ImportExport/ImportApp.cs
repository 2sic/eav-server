using System;
using System.Collections.Generic;
using System.IO;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Environment;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Apps.ImportExport.ImportHelpers;
using ToSic.Eav.Configuration;
using ToSic.Eav.Context;
using ToSic.Eav.Identity;
using ToSic.Lib.Logging;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.ImportExport
{
    public class ImportApp: HasLog<ImportApp>
    {
        #region DI Constructor

        public ImportApp(IEnvironmentLogger envLogger, ZipImport zipImport, IGlobalConfiguration globalConfiguration, IUser user, AppFinder appFinder, ISite site, Lazy<XmlImportWithFiles> xmlImpExpFilesLazy) : base("Bck.Export")
        {
            _envLogger = envLogger;
            _zipImport = zipImport;
            _globalConfiguration = globalConfiguration;
            _user = user;
            _appFinder = appFinder;
            _site = site;
            _xmlImpExpFilesLazy = xmlImpExpFilesLazy;
        }

        private readonly IEnvironmentLogger _envLogger;
        private readonly ZipImport _zipImport;
        private readonly IGlobalConfiguration _globalConfiguration;
        private readonly IUser _user;
        private readonly AppFinder _appFinder;
        private readonly ISite _site;
        private readonly Lazy<XmlImportWithFiles> _xmlImpExpFilesLazy;

        #endregion

        public ImportResultDto Import(Stream stream, int zoneId, string renameApp)
        {
            Log.A("import app start");
            var result = new ImportResultDto();

            if (!string.IsNullOrEmpty(renameApp)) Log.A($"new app name: {renameApp}");

            var zipImport = _zipImport;
            try
            {
                zipImport.Init(zoneId, null, _user.IsSystemAdmin, Log);
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
                    var importer = _xmlImpExpFilesLazy.Value.Init(null, false, Log);
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
    }
}
