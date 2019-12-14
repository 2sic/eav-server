using System;
using System.Collections.Generic;
using System.IO;
using ToSic.Eav.Apps.ImportExport.ImportHelpers;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Zip;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.Apps.ImportExport
{
    public class ZipImport: HasLog
    {
        private readonly int? _initialAppId;
        private readonly int _zoneId;
        private readonly IImportExportEnvironment _environment;

        public readonly bool AllowCodeImport;

        public ZipImport(IImportExportEnvironment environment, int zoneId, int? appId, bool allowCode, ILog parentLog)
            :base("Zip.Imp", parentLog)
        {
            _initialAppId = appId;
            _zoneId = zoneId;
            _environment = environment;
            AllowCodeImport = allowCode;
        }

        /// <summary>
        /// Imports a ZIP file (from stream)
        /// </summary>
        /// <param name="zipStream">zip file</param>
        /// <param name="temporaryDirectory">temporary storage</param>
        /// <param name="rename">App rename</param>
        /// <returns></returns>
        public bool ImportZip(Stream zipStream, string temporaryDirectory, string rename = null)
        {
            var wrapLog = Log.Call<bool>( nameof(ImportZip),$"{temporaryDirectory}, {nameof(rename)}:{rename}");
            var messages = _environment.Messages;
            Exception finalException = null;

            try
            {
                // ensure temp directory and unzip to there
                if (!Directory.Exists(temporaryDirectory))
                    Directory.CreateDirectory(temporaryDirectory);
                new Zipping(Log).ExtractZipFile(zipStream, temporaryDirectory, AllowCodeImport);

                // Loop through each root-folder.
                // For now only it should only contain the "Apps" folder.
                foreach (var directoryPath in Directory.GetDirectories(temporaryDirectory))
                {
                    Log.Add($"folder:{directoryPath}");
                    if (Path.GetFileName(directoryPath) != "Apps") continue;
                    var appDir = Path.Combine(temporaryDirectory, "Apps");
                    // Loop through each app directory
                    ImportApps(rename, appDir, messages);
                }
            }
            catch (IOException e)
            {
                // The app could not be installed because the app-folder already exists. Install app in different folder?
                finalException = e;
                // Add error message and return false, but use MessageTypes.Warning so we can prompt user for new different rename
                messages.Add(new Message("Could not import the app / package: " + e.Message, Message.MessageTypes.Warning));
            }
            catch (Exception e)
            {
                finalException = e; 
                // Add error message and return false
                messages.Add(new Message("Could not import the app / package: " + e.Message, Message.MessageTypes.Error));
            }
            finally
            {
                TryToCleanUpTemporary(temporaryDirectory);
            }

            if (finalException != null)
            {
                Log.Add("had found errors during import, will throw");
                wrapLog("error", false);
                throw finalException; // must throw, to enable logging outside
            }

            return wrapLog("ok", true);
        }

        private void TryToCleanUpTemporary(string temporaryDirectory)
        {
            var wrapLog = Log.Call(temporaryDirectory);
            try
            {
                // Finally delete the temporary directory
                Directory.Delete(temporaryDirectory, true);
                wrapLog("ok");
            }
            catch
            {
                Log.Add("Delete ran into issues, will ignore. " +
                        "Probably files/folders are used by another process like anti-virus. " +
                        "Just leave temp folder as is");
                wrapLog("error");
            }
        }

        private void ImportApps(string rename, string tempDir, List<Message> importMessages)
        {
            foreach (var appDirectory in Directory.GetDirectories(tempDir))
                ImportApp(rename, appDirectory, importMessages);
        }

        /// <summary>
        /// Import an app from temporary
        /// </summary>
        /// <remarks>
        /// Historical note: the xml file used to have a different rename
        /// but it's been app.xml since Oct 2016 so this is all we plan to support
        /// </remarks>
        /// <param name="rename"></param>
        /// <param name="appDirectory"></param>
        /// <param name="importMessages"></param>
        private void ImportApp(string rename, string appDirectory, List<Message> importMessages)
        {
            var wrapLog = Log.Call($"{nameof(rename)}:'{rename}', {nameof(appDirectory)}:'{appDirectory}', ...");
            
            // Import XML file(s)
            foreach (var xmlFileName in Directory.GetFiles(appDirectory, "App.xml"))
                ImportAppXml(rename, appDirectory, xmlFileName, importMessages);
            
            wrapLog("ok");
        }

        private void ImportAppXml(string rename, string appDirectory, string xmlFileName, List<Message> importMessages)
        {
            var wrapLog = Log.Call($"{nameof(rename)}:'{rename}' {nameof(appDirectory)}:'{appDirectory}', ...");
            
            int appId;
            var importer = new XmlImportWithFiles(Log);
            var imp = new ImportXmlReader(Path.Combine(appDirectory, xmlFileName), importer, Log);

            if (imp.IsAppImport)
            {
                Log.Add("will do app-import");

                // Version Checks (new in 08.03.03)
                new VersionCheck(_environment, Log).EnsureVersions(imp.AppConfig);

                var folder = imp.AppFolder;

                // user decided to install app in different folder, because same App is already installed
                if (!string.IsNullOrEmpty(rename))
                {
                    var renamer = new RenameOnImport(folder, rename, Log);
                    renamer.FixAppXmlForImportAsDifferentApp(imp);
                    renamer.FixPortalFilesAdamAppFolderName(appDirectory);
                    folder = rename;
                }

                // Throw error if the app directory already exists
                var appPath = _environment.TargetPath(folder);
                if (Directory.Exists(appPath))
                    throw new IOException($"App could not be installed, app-folder '{appPath}' already exists.");

                HandlePortalFilesFolder(appDirectory);

                importer.ImportApp(_zoneId, imp.XmlDoc, out appId);
            }
            else
            {
                Log.Add("will do content import");
                appId = _initialAppId ?? new ZoneRuntime(_zoneId, Log).DefaultAppId;

                if (importer.IsCompatible(imp.XmlDoc))
                    HandlePortalFilesFolder(appDirectory);

                importer.ImportXml(_zoneId, appId, imp.XmlDoc);
            }

            importMessages.AddRange(importer.Messages);
            FixOld2SexyFolderName(importMessages, appId, appDirectory);
            
            wrapLog("ok");
        }

        /// <summary>
        /// Copy all files in 2sexy folder to (portal file system) 2sexy folder
        /// </summary>
        /// <param name="importMessages"></param>
        /// <param name="appId"></param>
        /// <param name="appDirectory"></param>
        private void FixOld2SexyFolderName(List<Message> importMessages, int appId, string appDirectory)
        {
            var templateRoot = _environment.TemplatesRoot(_zoneId, appId);
            var appTemplateRoot = Path.Combine(appDirectory, "2sexy");
            if (Directory.Exists(appTemplateRoot))
                new FileManager(appTemplateRoot).CopyAllFiles(templateRoot, false, importMessages);
        }

        private void HandlePortalFilesFolder(string appDirectory)
        {
            // Handle PortalFiles folder
            var portalTempRoot = Path.Combine(appDirectory, XmlConstants.PortalFiles);
            if (Directory.Exists(portalTempRoot))
                _environment.TransferFilesToTenant(portalTempRoot, string.Empty);
        }

    }
}