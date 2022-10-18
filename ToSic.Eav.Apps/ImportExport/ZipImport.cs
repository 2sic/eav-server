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
            var wrapLog = Log.Fn<bool>( parameters: $"{temporaryDirectory}, {nameof(rename)}:{rename}");
            var messages = Messages;
            Exception finalException = null;

            try
            {
                // create temp directory unless exists
                Directory.CreateDirectory(temporaryDirectory);

                // unzip to temp directory
                new Zipping(Log).ExtractZipFile(zipStream, temporaryDirectory, AllowCodeImport);

                // Loop through each root-folder.
                // For now only it should only contain the "Apps" folder.
                foreach (var directoryPath in Directory.GetDirectories(temporaryDirectory))
                {
                    Log.A($"folder:{directoryPath}");
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
                // Finally delete the temporary directory
                TryToDeleteDirectory(temporaryDirectory, Log);
            }

            if (finalException != null)
            {
                Log.A("had found errors during import, will throw");
                wrapLog.ReturnFalse("error");
                throw finalException; // must throw, to enable logging outside
            }

            return wrapLog.ReturnTrue("ok");
        }


        /// <summary>
        /// Try to delete folder
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="log"></param>
        public static void TryToDeleteDirectory(string directoryPath, ILog log)
        {
            var wrapLog = log.Fn(directoryPath);
            try
            {
                if (Directory.Exists(directoryPath))
                    Directory.Delete(directoryPath, true);
                wrapLog.Done("ok");
            }
            catch(Exception e)
            {
                log.Ex(e);
                log.A("Delete ran into issues, will ignore. " +
                      "Probably files/folders are used by another process like anti-virus. " +
                      "Just leave folder as is");
                wrapLog.Done("error");
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
            var wrapLog = Log.Fn($"{nameof(rename)}:'{rename}', {nameof(appDirectory)}:'{appDirectory}', ...");

            // migrate old app.xml and 2sexy/.data/app.xml to 2sexy/App_Data
            MigrateForImportAppDataFile(appDirectory);

            // Import app.xml file(s) when is located in appDirectory/2sexy/App_Data
            foreach (var _ in Directory.GetFiles(Path.Combine(appDirectory, Constants.ToSxcFolder, Constants.AppDataProtectedFolder), Constants.AppDataFile))
                ImportAppXmlAndFiles(rename, appDirectory, importMessages);
            
            wrapLog.Done("ok");
        }

        private void ImportAppXmlAndFiles(string rename, string appDirectory, List<Message> importMessages)
        {
            var wrapLog = Log.Fn($"{nameof(rename)}:'{rename}' {nameof(appDirectory)}:'{appDirectory}', ...");
            
            int appId;
            var importer = _xmlImpExpFilesLazy.Value.Init(null, false, Log); // new XmlImportWithFiles(Log);

            var imp = new ImportXmlReader(Path.Combine(appDirectory, Constants.ToSxcFolder, Constants.AppDataProtectedFolder, Constants.AppDataFile), importer, Log);

            if (imp.IsAppImport)
            {
                Log.A("will do app-import");

                // Version Checks (new in 08.03.03)
                new VersionCheck(Env, Log).EnsureVersions(imp.AppConfig);

                var folder = imp.AppFolder;
                
                // user decided to install app in different folder, because same App is already installed
                if (!string.IsNullOrEmpty(rename))
                {
                    Log.A($"User rename to '{rename}'");
                    var renamer = new RenameOnImport(folder, rename, Log);
                    renamer.FixAppXmlForImportAsDifferentApp(imp);
                    renamer.FixPortalFilesAdamAppFolderName(appDirectory);
                    folder = rename;
                }
                else Log.A("No rename of app requested");

                // Throw error if the app directory already exists
                var appPath = Env.TargetPath(folder);
                if (Directory.Exists(appPath))
                    throw new IOException($"App could not be installed, app-folder '{appPath}' already exists.");

                HandlePortalFilesFolder(appDirectory);

                importer.ImportApp(_zoneId, imp.XmlDoc, out appId);
            }
            else
            {
                Log.A("will do content import");
                appId = _initialAppId ?? _appStates.DefaultAppId(_zoneId);

                if (importer.IsCompatible(imp.XmlDoc))
                    HandlePortalFilesFolder(appDirectory);

                importer.ImportXml(_zoneId, appId, imp.XmlDoc);
            }

            importMessages.AddRange(importer.Messages);
            CopyAppFiles(importMessages, appId, appDirectory);
            CopyAppGlobalFiles(importMessages, appId, appDirectory);

            // New in V11 - now that we just imported content types into the /system folder
            // the App must be refreshed to ensure these are available for working
            // Must happen after CopyAppFiles(...)
            _systemManager.Init(Log).PurgeApp(appId);

            wrapLog.Done("ok");
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
            var wrapLog = Log.Fn($"..., {appId}, {tempFolder}");
            var templateRoot = Env.TemplatesRoot(_zoneId, appId);
            var appTemplateRoot = Path.Combine(tempFolder, Constants.ZipFolderForAppStuff);
            if (Directory.Exists(appTemplateRoot))
                new FileManager(appTemplateRoot).Init(Log).CopyAllFiles(templateRoot, false, importMessages);
            wrapLog.Done("ok");
        }

        /// <summary>
        /// Copy all files in 2sexyGlobal folder to global 2sexy folder
        /// </summary>
        /// <param name="importMessages"></param>
        /// <param name="appId"></param>
        /// <param name="tempFolder"></param>
        /// <param name="deleteGlobalTemplates"></param>
        /// <param name="overwriteFiles"></param>
        /// <remarks>The zip file still uses the "2sexyGlobal" folder name instead of "2sxcGlobal"</remarks>
        public void CopyAppGlobalFiles(List<Message> importMessages, int appId, string tempFolder, bool deleteGlobalTemplates = false, bool overwriteFiles = false)
        {
            var wrapLog = Log.Fn($"..., {appId}, {tempFolder}, {deleteGlobalTemplates}, {overwriteFiles}");
            var globalTemplatesRoot = Env.GlobalTemplatesRoot(_zoneId, appId);
            var appTemplateRoot = Path.Combine(tempFolder, Constants.ZipFolderForGlobalAppStuff);
            if (Directory.Exists(appTemplateRoot))
            {
                if (deleteGlobalTemplates) 
                    TryToDeleteDirectory(globalTemplatesRoot, Log);

                Log.A("copy all files to app global template folder");
                new FileManager(appTemplateRoot).Init(Log).CopyAllFiles(globalTemplatesRoot, overwriteFiles, importMessages);
            }

            wrapLog.Done("ok");
        }

        private void HandlePortalFilesFolder(string appDirectory)
        {
            var wrapLog = Log.Fn();
            // Handle PortalFiles folder
            var portalTempRoot = Path.Combine(appDirectory, XmlConstants.PortalFiles);
            if (Directory.Exists(portalTempRoot))
            {
                var messages = Env.TransferFilesToSite(portalTempRoot, string.Empty);
                Messages.AddRange(messages);
            }
            wrapLog.Done();
        }

        /// <summary>
        /// for import only, migrate app.xml or old 2sexy/.data/app.xml to 2sexy/App_Data
        /// </summary>
        /// <param name="appRootPath"></param>
        /// <param name="xmlFileName"></param>
        public static void MigrateForImportAppDataFile(string appRootPath)
        {
            var oldAppFilePath = Path.Combine(appRootPath, Constants.AppDataFile);
            var oldDataAppFilePath = Path.Combine(appRootPath, Constants.ToSxcFolder, Constants.FolderData, Constants.AppDataFile);
            if (!File.Exists(oldAppFilePath) && !File.Exists(oldDataAppFilePath)) return;

            Directory.CreateDirectory(Path.Combine(appRootPath, Constants.ToSxcFolder, Constants.AppDataProtectedFolder));
            var newFilePath = Path.Combine(appRootPath, Constants.ToSxcFolder, Constants.AppDataProtectedFolder, Constants.AppDataFile);

            if (File.Exists(oldDataAppFilePath))
            {
                if (File.Exists(newFilePath)) File.Delete(newFilePath);
                File.Move(oldDataAppFilePath, newFilePath);
            }

            if (File.Exists(oldAppFilePath))
            {
                if (File.Exists(newFilePath)) File.Delete(newFilePath);
                File.Move(oldAppFilePath, newFilePath);
            }
        }


        /// <summary>
        /// migrate old .data/app.xml to App_Data
        /// </summary>
        /// <param name="appRootPath"></param>
        public static void MigrateOldAppDataFile(string appRootPath)
        {
            var oldDataAppFilePath = Path.Combine(appRootPath, Constants.FolderData, Constants.AppDataFile);
            if (!File.Exists(oldDataAppFilePath)) return;

            Directory.CreateDirectory(Path.Combine(appRootPath, Constants.AppDataProtectedFolder));
            var newFilePath = Path.Combine(appRootPath, Constants.AppDataProtectedFolder, Constants.AppDataFile);

            if (File.Exists(newFilePath)) File.Delete(newFilePath);
            File.Move(oldDataAppFilePath, newFilePath);

            //// commented cody will move/copy .data to App_Data with all content
            //var oldFolderPath = Path.Combine(appRootPath, Eav.Constants.FolderData);
            //if (!Directory.Exists(oldFolderPath)) return;

            //var newFolderPath = Path.Combine(appRootPath, Eav.Constants.AppDataProtectedFolder);
            //if (!Directory.Exists(newFolderPath))
            //{
            //    Directory.Move(oldFolderPath, newFolderPath);
            //}
            //else
            //{
            //    CopyDirectory(oldFolderPath, newFolderPath, true);
            //    Directory.Delete(oldFolderPath, true);
            //}
        }

        ///// <summary>
        ///// copy directory with its content
        ///// based on https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        ///// </summary>
        ///// <param name="sourceDir"></param>
        ///// <param name="destinationDir"></param>
        ///// <param name="recursive"></param>
        ///// <exception cref="DirectoryNotFoundException"></exception>
        //private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        //{
        //    // Get information about the source directory
        //    var dir = new DirectoryInfo(sourceDir);

        //    // Check if the source directory exists
        //    if (!dir.Exists)
        //        return; //throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        //    // Cache directories before we start copying
        //    var dirs = dir.GetDirectories();

        //    // Create the destination directory
        //    Directory.CreateDirectory(destinationDir);

        //    // Get the files in the source directory and copy to the destination directory
        //    foreach (var file in dir.GetFiles())
        //        file.CopyTo(Path.Combine(destinationDir, file.Name));

        //    if (!recursive) return;

        //    // If recursive and copying subdirectories, recursively call this method
        //    foreach (var subDir in dirs)
        //        CopyDirectory(subDir.FullName, Path.Combine(destinationDir, subDir.Name), true);
        //}

    }
}