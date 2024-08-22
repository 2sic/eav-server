using System.IO;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.ImportExport.Internal.ImportHelpers;
using ToSic.Eav.Internal.Environment;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.ImportExport.Internal.Zip;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ZipImport(ZipImport.MyServices services) : ServiceBase<ZipImport.MyServices>(services, "Zip.Imp")
{
    private int? _initialAppId;
    private int _zoneId;

    public List<Message> Messages { get; } = [];

    public bool AllowCodeImport;

    public class MyServices(
        Generator<FileManager> fileManagerGenerator,
        IImportExportEnvironment environment,
        Generator<XmlImportWithFiles> xmlImpExpFiles,
        AppCachePurger appCachePurger,
        IAppStates appStates)
        : MyServicesBase(connect: [fileManagerGenerator, environment, xmlImpExpFiles, appCachePurger, appStates])
    {
        public Generator<FileManager> FileManagerGenerator { get; } = fileManagerGenerator;
        public IImportExportEnvironment Environment { get; } = environment;
        public Generator<XmlImportWithFiles> XmlImpExpFiles { get; } = xmlImpExpFiles;
        public AppCachePurger AppCachePurger { get; } = appCachePurger;
        public IAppStates AppStates { get; } = appStates;
    }


    public ZipImport Init(int zoneId, int? appId, bool allowCode)
    {
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
        => ImportZipInternal(temporaryDirectory, rename, zipStream);

    /// <summary>
    /// Imports a ZIP file (from file)
    /// </summary>
    /// <param name="zipPath">path to ZIP file</param>
    /// <param name="temporaryDirectory">temporary storage</param>
    /// <param name="rename">App rename</param>
    /// <param name="inheritAppId">optional inherit AppId</param>
    /// <returns></returns>
    public bool ImportZip(string zipPath, string temporaryDirectory, string rename = null, int? inheritAppId = null)
        => ImportZipInternal(temporaryDirectory, rename, null, zipPath, inheritAppId);

    private bool ImportZipInternal(string temporaryDirectory, string rename = null, Stream zipStream = null, string zipPath = null, int? inheritAppId = null)
    {
        var l = Log.Fn<bool>(parameters: $"{temporaryDirectory}, {nameof(rename)}:{rename}, {nameof(zipPath)}:{zipPath}, {nameof(inheritAppId)}:{inheritAppId}");
        Exception finalException = null;

        try
        {
            // create temp directory unless exists
            Directory.CreateDirectory(temporaryDirectory);

            // unzip to temp directory
            if (zipStream != null)
            {
                l.A("will extract zip from stream");
                new Zipping(Log).ExtractZipStream(zipStream, temporaryDirectory, AllowCodeImport);
            }
            else if (zipPath != null)
            {
                l.A($"will extract zip from file: {zipPath}");
                new Zipping(Log).ExtractZipFile(zipPath, temporaryDirectory, AllowCodeImport);
            }
            else
                throw new ArgumentNullException("zipStream or zipPath");

            // Loop through each root-folder.
            // For now only it should only contain the "Apps" folder.
            foreach (var directoryPath in Directory.GetDirectories(temporaryDirectory))
            {
                l.A($"folder:{directoryPath}");
                if (Path.GetFileName(directoryPath) != "Apps") continue;
                var packageDir = Path.Combine(temporaryDirectory, "Apps");
                // Loop through each app directory
                foreach (var appDirectory in Directory.GetDirectories(packageDir))
                    ImportApp(rename, appDirectory, Messages, pendingApp: false, inheritAppId);
            }
        }
        catch (IOException e)
        {
            // The app could not be installed because the app-folder already exists. Install app in different folder?
            finalException = e;
            // Add error message and return false, but use MessageTypes.Warning so we can prompt user for new different rename
            Messages.Add(new("Could not import the app / package: " + e.Message, Message.MessageTypes.Warning));
        }
        catch (Exception e)
        {
            finalException = e;
            // Add error message and return false
            Messages.Add(new("Could not import the app / package: " + e.Message, Message.MessageTypes.Error));
        }
        finally
        {
            // Finally delete the temporary directory
            TryToDeleteDirectory(temporaryDirectory, Log);
        }

        if (finalException != null)
        {
            l.A("had found errors during import, will throw");
            l.ReturnFalse("error");
            throw finalException; // must throw, to enable logging outside
        }

        return l.ReturnTrue("ok");
    }


    /// <summary>
    /// Try to delete folder
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <param name="log"></param>
    public static void TryToDeleteDirectory(string directoryPath, ILog log)
    {
        var l = log.Fn($"{nameof(directoryPath)}:'{directoryPath}'");
        var retryDelete = 0;
        do
        {
            try
            {
                if (Directory.Exists(directoryPath))
                    Directory.Delete(directoryPath, true);
            }
            catch (Exception e)
            {
                ++retryDelete;
                log.Ex(e);
                log.A("Delete ran into issues, will ignore. " +
                      "Probably files/folders are used by another process like anti-virus. " +
                      $"Retry: {retryDelete}.");
            }
        } while (Directory.Exists(directoryPath) && retryDelete <= 20);

        l.Done(Directory.Exists(directoryPath) ? "error, can't delete" : "ok");
    }


    /// <summary>
    /// Import an app from directory
    /// </summary>
    /// <remarks>
    /// Historical note: the xml file used to have a different rename
    /// but it's been app.xml since Oct 2016 so this is all we plan to support
    /// </remarks>
    /// <param name="rename"></param>
    /// <param name="appDirectory"></param>
    /// <param name="importMessages"></param>
    /// <param name="pendingApp"></param>
    /// <param name="inheritAppId">optional inherit appId</param>
    public bool ImportApp(string rename, string appDirectory, List<Message> importMessages, bool pendingApp, int? inheritAppId = null)
    {
        var l = Log.Fn<bool>($"{nameof(rename)}:'{rename}', {nameof(appDirectory)}:'{appDirectory}', ...");
        try
        {
            // migrate old app.xml and 2sexy/.data/app.xml to 2sexy/App_Data
            MigrateForImportAppDataFile(appDirectory);

            // Import app.xml file(s) when is located in appDirectory/2sexy/App_Data
            foreach (var _ in Directory.GetFiles(AppDataProtectedFolderPath(appDirectory, pendingApp), Constants.AppDataFile))
                ImportAppXmlAndFiles(rename, appDirectory, importMessages, pendingApp, inheritAppId);
        }
        catch (Exception e)
        {
            l.A("had found errors during import, will throw");
            l.Done(e);
            throw; // must throw, to enable logging outside
        }
        return l.ReturnTrue("ok");
    }

    private void ImportAppXmlAndFiles(string rename, string appDirectory, List<Message> importMessages, bool pendingApp, int? inheritAppId = null)
    {
        var l = Log.Fn($"{nameof(rename)}:'{rename}' {nameof(appDirectory)}:'{appDirectory}', ...");

        int appId;
        var importer = Services.XmlImpExpFiles.New().Init(null, false);

        var imp = new ImportXmlReader(
            Path.Combine(AppDataProtectedFolderPath(appDirectory, pendingApp), Constants.AppDataFile), importer,
            l);

        if (imp.IsAppImport)
        {
            l.A("will do app-import");

            // Version Checks (new in 08.03.03)
            // todo: register in DI and add to dependencies, then remove Init(log)
            new VersionCheck(Services.Environment, l).EnsureVersions(imp.AppConfig);

            var folder = imp.AppFolder;

            // user decided to install app in different folder, because same App is already installed
            if (!string.IsNullOrEmpty(rename))
            {
                l.A($"User rename to '{rename}'");
                var renameHelper = new RenameOnImport(folder, rename, l);
                renameHelper.FixAppXmlForImportAsDifferentApp(imp);
                renameHelper.FixPortalFilesAdamAppFolderName(appDirectory, pendingApp);
                folder = rename;
            }
            else l.A("No rename of app requested");

            if (!pendingApp)
            {
                // Throw error if the app directory already exists
                var appPath = Services.Environment.TargetPath(folder);
                if (Directory.Exists(appPath))
                    throw new IOException($"App could not be installed, app-folder '{appPath}' already exists.");
            }

            HandlePortalFilesFolder(appDirectory, pendingApp);

            importer.ImportApp(_zoneId, imp.XmlDoc, inheritAppId, out appId);
        }
        else
        {
            l.A("will do content import");
            appId = _initialAppId ?? Services.AppStates.AppsCatalog.DefaultAppIdentity(_zoneId).AppId;

            if (importer.IsCompatible(imp.XmlDoc))
                HandlePortalFilesFolder(appDirectory, pendingApp);

            importer.ImportXml(_zoneId, appId, parentAppId: null /* not sure if we never have a parent here */, imp.XmlDoc);
        }

        importMessages.AddRange(importer.Messages);
        if (!pendingApp) CopyAppFiles(importMessages, appId, appDirectory);

        var tmpAppGlobalFilesRoot = pendingApp ? Path.Combine(appDirectory, Constants.AppDataProtectedFolder) : appDirectory;
        CopyAppGlobalFiles(importMessages, appId, tmpAppGlobalFilesRoot, deleteGlobalTemplates: false, overwriteFiles: true);
        // New in V11 - now that we just imported content types into the /system folder
        // the App must be refreshed to ensure these are available for working
        // Must happen after CopyAppFiles(...)
        Services.AppCachePurger.PurgeApp(appId);

        l.Done("ok");
    }

    private static string AppDataProtectedFolderPath(string appDirectory, bool pendingApp)
        => pendingApp
            ? Path.Combine(appDirectory, Constants.AppDataProtectedFolder)
            : Path.Combine(appDirectory, Constants.ToSxcFolder, Constants.AppDataProtectedFolder);

    /// <summary>
    /// Copy all files in 2sexy folder to (portal file system) 2sexy folder
    /// </summary>
    /// <param name="importMessages"></param>
    /// <param name="appId"></param>
    /// <param name="tempFolder"></param>
    /// <remarks>The zip file still uses the old "2sexy" folder name instead of "2sxc"</remarks>
    private void CopyAppFiles(List<Message> importMessages, int appId, string tempFolder)
    {
        var l = Log.Fn($"..., {appId}, {tempFolder}");
        var templateRoot = Services.Environment.TemplatesRoot(_zoneId, appId);
        var appTemplateRoot = Path.Combine(tempFolder, Constants.ZipFolderForAppStuff);
        if (Directory.Exists(appTemplateRoot))
            base.Services.FileManagerGenerator.New().SetFolder(appId, appTemplateRoot).CopyAllFiles(templateRoot, false, importMessages);
        l.Done("ok");
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
        var l = Log.Fn($"..., {appId}, {tempFolder}, {deleteGlobalTemplates}, {overwriteFiles}");
        var globalTemplatesRoot = Services.Environment.GlobalTemplatesRoot(_zoneId, appId);
        var appTemplateRoot = Path.Combine(tempFolder, Constants.ZipFolderForGlobalAppStuff);
        if (Directory.Exists(appTemplateRoot))
        {
            if (deleteGlobalTemplates)
                TryToDeleteDirectory(globalTemplatesRoot, l);

            l.A("copy all files to app global template folder");
            base.Services.FileManagerGenerator.New()
                .SetFolder(appId, appTemplateRoot)
                .CopyAllFiles(globalTemplatesRoot, overwriteFiles, importMessages);
        }
        l.Done("ok");
    }

    private void HandlePortalFilesFolder(string appDirectory, bool pendingApp)
    {
        var l = Log.Fn($"{nameof(appDirectory)}:'{appDirectory}', {nameof(pendingApp)}:{pendingApp}");
        // Handle PortalFiles/SiteFiles folder
        var portalTempRoot = pendingApp
            ? Path.Combine(appDirectory, Constants.AppDataProtectedFolder, Constants.ZipFolderForSiteFiles)
            : Path.Combine(appDirectory,
                Constants.ZipFolderForPortalFiles); // TODO: probably replace with Constants.ZipFolderForSiteFiles
        l.A($"{nameof(portalTempRoot)}:{portalTempRoot}");

        if (Directory.Exists(portalTempRoot))
        {
            var messages = Services.Environment.TransferFilesToSite(portalTempRoot, string.Empty);
            foreach (var message in messages) l.A(message.Text);
            Messages.AddRange(messages);
        }
        l.Done("ok");
    }

    /// <summary>
    /// for import only, migrate app.xml or old 2sexy/.data/app.xml to 2sexy/App_Data
    /// </summary>
    /// <param name="appRootPath"></param>
    /// <param name="xmlFileName"></param>
    public static void MigrateForImportAppDataFile(string appRootPath)
    {
        var oldAppFilePath = Path.Combine(appRootPath, Constants.AppDataFile);
        var oldDataAppFilePath = Path.Combine(appRootPath, Constants.ToSxcFolder, Constants.FolderOldDotData, Constants.AppDataFile);
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
        var oldDataAppFilePath = Path.Combine(appRootPath, Constants.FolderOldDotData, Constants.AppDataFile);
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