using System;
using System.Collections.Generic;
using System.IO;
using ToSic.Eav.Apps.ImportExport.ImportHelpers;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Zip;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.Apps.ImportExport
{
    public class ZipImport: HasLog
    {
        private readonly Lazy<XmlImportWithFiles> _xmlImpExpFilesLazy;
        private readonly IAppStates _appStates;
        private readonly SystemManager _systemManager;
        private int? _initialAppId;
        private int _zoneId;
        public readonly IImportExportEnvironment Env;

        public List<Message> Messages { get; }

        public bool AllowCodeImport;

        public ZipImport(IImportExportEnvironment environment, Lazy<XmlImportWithFiles> xmlImpExpFilesLazy, SystemManager systemManager, IAppStates appStates) :base("Zip.Imp")
        {
            _xmlImpExpFilesLazy = xmlImpExpFilesLazy;
            _appStates = appStates;
            _systemManager = systemManager.Init(Log);
            Env = environment.Init(Log);
            Messages = new List<Message>();
        }

        public ZipImport Init(int zoneId, int? appId, bool allowCode, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            _initialAppId = appId;
            _zoneId = zoneId;
            AllowCodeImport = allowCode;
            return this;
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
            var wrapLog = Log.Call<bool>( parameters: $"{temporaryDirectory}, {nameof(rename)}:{rename}");
            var messages = Messages;
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
                    var packageDir = Path.Combine(temporaryDirectory, "Apps");
                    // Loop through each app directory
                    foreach (var appDirectory in Directory.GetDirectories(packageDir))
                        ImportApp(rename, appDirectory, messages);

                    //ImportApps(rename, packageDir, messages);
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
                ImportAppXmlAndFiles(rename, appDirectory, xmlFileName, importMessages);
            
            wrapLog("ok");
        }

        private void ImportAppXmlAndFiles(string rename, string appDirectory, string xmlFileName, List<Message> importMessages)
        {
            var wrapLog = Log.Call($"{nameof(rename)}:'{rename}' {nameof(appDirectory)}:'{appDirectory}', ...");
            
            int appId;
            var importer = _xmlImpExpFilesLazy.Value.Init(null, false, Log); // new XmlImportWithFiles(Log);
            var imp = new ImportXmlReader(Path.Combine(appDirectory, xmlFileName), importer, Log);

            if (imp.IsAppImport)
            {
                Log.Add("will do app-import");

                // Version Checks (new in 08.03.03)
                new VersionCheck(Env, Log).EnsureVersions(imp.AppConfig);

                var folder = imp.AppFolder;

                // user decided to install app in different folder, because same App is already installed
                if (!string.IsNullOrEmpty(rename))
                {
                    Log.Add($"User rename to '{rename}'");
                    var renamer = new RenameOnImport(folder, rename, Log);
                    renamer.FixAppXmlForImportAsDifferentApp(imp);
                    renamer.FixPortalFilesAdamAppFolderName(appDirectory);
                    folder = rename;
                }
                else Log.Add("No rename of app requested");

                // Throw error if the app directory already exists
                var appPath = Env.TargetPath(folder);
                if (Directory.Exists(appPath))
                    throw new IOException($"App could not be installed, app-folder '{appPath}' already exists.");

                HandlePortalFilesFolder(appDirectory);

                importer.ImportApp(_zoneId, imp.XmlDoc, out appId);
            }
            else
            {
                Log.Add("will do content import");
                appId = _initialAppId ?? _appStates.DefaultAppId(_zoneId);

                if (importer.IsCompatible(imp.XmlDoc))
                    HandlePortalFilesFolder(appDirectory);

                importer.ImportXml(_zoneId, appId, imp.XmlDoc);
            }

            importMessages.AddRange(importer.Messages);
            CopyAppFiles(importMessages, appId, appDirectory);

            // New in V11 - now that we just imported content types into the /system folder
            // the App must be refreshed to ensure these are available for working
            // Must happen after CopyAppFiles(...)
            _systemManager.Init(Log).PurgeApp(appId);

            wrapLog("ok");
        }

        /// <summary>
        /// Copy all files in 2sexy folder to (portal file system) 2sexy folder
        /// </summary>
        /// <param name="importMessages"></param>
        /// <param name="appId"></param>
        /// <param name="tempFolder"></param>
        /// <remarks>The zip file still uses the old "2sexy" folder name instead of "2sxc"</remarks>
        private void CopyAppFiles(List<Message> importMessages, int appId, string tempFolder)
        {
            var wrapLog = Log.Call($"..., {appId}, {tempFolder}");
            var templateRoot = Env.TemplatesRoot(_zoneId, appId);
            var appTemplateRoot = Path.Combine(tempFolder, "2sexy");
            if (Directory.Exists(appTemplateRoot))
                new FileManager(appTemplateRoot).CopyAllFiles(templateRoot, false, importMessages);
            wrapLog("ok");
        }

        private void HandlePortalFilesFolder(string appDirectory)
        {
            var wrapLog = Log.Call();
            // Handle PortalFiles folder
            var portalTempRoot = Path.Combine(appDirectory, XmlConstants.PortalFiles);
            if (Directory.Exists(portalTempRoot))
            {
                var messages = Env.TransferFilesToSite(portalTempRoot, string.Empty);
                Messages.AddRange(messages);
            }
            wrapLog(null);
        }

    }
}