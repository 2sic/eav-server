﻿using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.ImportExport.Integration;
using ToSic.Eav.ImportExport.Sys.ImportHelpers;
using ToSic.Eav.ImportExport.Sys.XmlImport;
using ToSic.Eav.Persistence.Sys.Logging;
using ToSic.Eav.Sys;

namespace ToSic.Eav.ImportExport.Sys.Zip;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ZipImport(ZipImport.Dependencies services) : ServiceBase<ZipImport.Dependencies>(services, "Zip.Imp")
{
    private int? _initialAppId;
    private int _zoneId;

    public List<Message> Messages { get; } = [];

    public bool AllowCodeImport;

    public class Dependencies(
        Generator<AppFileManager> fileManagerGenerator,
        IImportExportEnvironment environment,
        Generator<XmlImportWithFiles> xmlImpExpFiles,
        AppCachePurger appCachePurger,
        IAppsCatalog appsCatalog)
        : DependenciesBase(connect: [fileManagerGenerator, environment, xmlImpExpFiles, appCachePurger, appsCatalog])
    {
        public Generator<AppFileManager> FileManagerGenerator { get; } = fileManagerGenerator;
        public IImportExportEnvironment Environment { get; } = environment;
        public Generator<XmlImportWithFiles> XmlImpExpFiles { get; } = xmlImpExpFiles;
        public AppCachePurger AppCachePurger { get; } = appCachePurger;
        public IAppsCatalog AppsCatalog { get; } = appsCatalog;
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
    /// <param name="newName">App rename</param>
    /// <returns></returns>
    public bool ImportZip(Stream zipStream, string temporaryDirectory, string? newName = null)
        => ImportZipInternal(temporaryDirectory, newName, zipStream);

    /// <summary>
    /// Imports a ZIP file (from file)
    /// </summary>
    /// <param name="zipPath">path to ZIP file</param>
    /// <param name="temporaryDirectory">temporary storage</param>
    /// <param name="newName">App rename</param>
    /// <param name="inheritAppId">optional inherit AppId</param>
    /// <returns></returns>
    public bool ImportZip(string zipPath, string temporaryDirectory, string? newName = null, int? inheritAppId = null)
        => ImportZipInternal(temporaryDirectory, newName, null, zipPath, inheritAppId);

    private bool ImportZipInternal(string temporaryDirectory, string? newName = null, Stream? zipStream = null, string? zipPath = null, int? inheritAppId = null)
    {
        var l = Log.Fn<bool>(parameters: $"{temporaryDirectory}, {nameof(newName)}:{newName}, {nameof(zipPath)}:{zipPath}, {nameof(inheritAppId)}:{inheritAppId}");
        Exception? finalException = null;

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
                throw new ArgumentNullException(nameof(zipPath), @"zipStream or zipPath");

            // Loop through each root-folder.
            // For now only it should only contain the "Apps" folder.
            foreach (var directoryPath in Directory.GetDirectories(temporaryDirectory))
            {
                l.A($"folder:{directoryPath}");
                if (Path.GetFileName(directoryPath) != "Apps")
                    continue;
                var packageDir = Path.Combine(temporaryDirectory, "Apps");
                // Loop through each app directory
                foreach (var appDirectory in Directory.GetDirectories(packageDir))
                    ImportApp(newName, appDirectory, Messages, pendingApp: false, inheritAppId);
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
                l.Ex(e);
                l.A("Delete ran into issues, will ignore. " +
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
    /// <param name="reName"></param>
    /// <param name="appDirectory"></param>
    /// <param name="importMessages"></param>
    /// <param name="pendingApp"></param>
    /// <param name="inheritAppId">optional inherit appId</param>
    public bool ImportApp(string? reName, string appDirectory, List<Message> importMessages, bool pendingApp, int? inheritAppId = null)
    {
        var l = Log.Fn<bool>($"{nameof(reName)}:'{reName}', {nameof(appDirectory)}:'{appDirectory}', ...");
        try
        {
            // migrate old app.xml and 2sexy/.data/app.xml to 2sexy/App_Data
            MigrateForImportAppDataFile(appDirectory);

            // Import app.xml file(s) when is located in appDirectory/2sexy/App_Data
            foreach (var _ in Directory.GetFiles(AppDataProtectedFolderPath(appDirectory, pendingApp), FolderConstants.AppDataFile))
                ImportAppXmlAndFiles(reName, appDirectory, importMessages, pendingApp, inheritAppId);
        }
        catch (Exception e)
        {
            l.A("had found errors during import, will throw");
            l.Done(e);
            throw; // must throw, to enable logging outside
        }
        return l.ReturnTrue("ok");
    }

    private void ImportAppXmlAndFiles(string? reName, string appDirectory, List<Message> importMessages, bool pendingApp, int? inheritAppId = null)
    {
        var l = Log.Fn($"{nameof(reName)}:'{reName}' {nameof(appDirectory)}:'{appDirectory}', ...");

        int appId;
        var importer = Services.XmlImpExpFiles.New().Init(null, false);

        var imp = new ImportXmlReader(
            Path.Combine(AppDataProtectedFolderPath(appDirectory, pendingApp), FolderConstants.AppDataFile),
            importer,
            l
        );

        if (imp.IsAppImport)
        {
            l.A("will do app-import");

            // Version Checks (new in 08.03.03)
            // todo: register in DI and add to dependencies, then remove Init(log)
            new VersionCheck(Services.Environment, l).EnsureVersions(imp.AppConfig);

            var folder = imp.AppFolder;

            // user decided to install app in different folder, because same App is already installed
            if (!string.IsNullOrEmpty(reName))
            {
                l.A($"User rename to '{reName}'");
                var renameHelper = new RenameOnImport(folder, reName!, l);
                renameHelper.FixAppXmlForImportAsDifferentApp(imp);
                renameHelper.FixPortalFilesAdamAppFolderName(appDirectory, pendingApp);
                folder = reName!;
            }
            else
                l.A("No rename of app requested");

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
            appId = _initialAppId ?? Services.AppsCatalog.DefaultAppIdentity(_zoneId).AppId;

            if (importer.IsCompatible(imp.XmlDoc))
                HandlePortalFilesFolder(appDirectory, pendingApp);

            importer.ImportXml(_zoneId, appId, parentAppId: null /* not sure if we never have a parent here */, imp.XmlDoc);
        }

        importMessages.AddRange(importer.Messages);
        if (!pendingApp) CopyAppFiles(importMessages, appId, appDirectory);

        var tmpAppGlobalFilesRoot = pendingApp
            ? Path.Combine(appDirectory, FolderConstants.DataFolderProtected)
            : appDirectory;
        CopyAppGlobalFiles(importMessages, appId, tmpAppGlobalFilesRoot, deleteGlobalTemplates: false, overwriteFiles: true);
        // New in V11 - now that we just imported content types into the /system folder
        // the App must be refreshed to ensure these are available for working
        // Must happen after CopyAppFiles(...)
        Services.AppCachePurger.PurgeApp(appId);

        l.Done("ok");
    }

    private static string AppDataProtectedFolderPath(string appDirectory, bool pendingApp)
        => pendingApp
            ? Path.Combine(appDirectory, FolderConstants.DataFolderProtected)
            : Path.Combine(appDirectory, FolderConstants.ToSxcFolder, FolderConstants.DataFolderProtected);

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
        var appTemplateRoot = Path.Combine(tempFolder, FolderConstants.ZipFolderForAppStuff);
        if (Directory.Exists(appTemplateRoot))
            Services.FileManagerGenerator.New().SetFolder(appId, appTemplateRoot).CopyAllFiles(templateRoot, false, importMessages);
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
        var appTemplateRoot = Path.Combine(tempFolder, FolderConstants.ZipFolderForGlobalAppStuff);
        if (Directory.Exists(appTemplateRoot))
        {
            if (deleteGlobalTemplates)
                TryToDeleteDirectory(globalTemplatesRoot, l);

            l.A("copy all files to app global template folder");
            Services.FileManagerGenerator.New()
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
            ? Path.Combine(appDirectory, FolderConstants.DataFolderProtected, FolderConstants.ZipFolderForSiteFiles)
            : Path.Combine(appDirectory,
                FolderConstants.ZipFolderForPortalFiles); // TODO: probably replace with Constants.ZipFolderForSiteFiles
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
    private static void MigrateForImportAppDataFile(string appRootPath)
    {
        var oldAppXmlFilePath = Path.Combine(appRootPath, FolderConstants.AppDataFile);
        var sxcPathInZipData = Path.Combine(appRootPath, FolderConstants.ToSxcFolder);
        var oldDataAppFilePath = Path.Combine(sxcPathInZipData, FolderConstants.DataFolderOld, FolderConstants.AppDataFile);
        if (!File.Exists(oldAppXmlFilePath) && !File.Exists(oldDataAppFilePath))
            return;

        var pathOfProtectedFolder = Path.Combine(sxcPathInZipData, FolderConstants.DataFolderProtected);
        Directory.CreateDirectory(pathOfProtectedFolder);
        var newFilePath = Path.Combine(pathOfProtectedFolder, FolderConstants.AppDataFile);

        if (File.Exists(oldDataAppFilePath))
        {
            if (File.Exists(newFilePath))
                File.Delete(newFilePath);
            File.Move(oldDataAppFilePath, newFilePath);
        }

        if (File.Exists(oldAppXmlFilePath))
        {
            if (File.Exists(newFilePath))
                File.Delete(newFilePath);
            File.Move(oldAppXmlFilePath, newFilePath);
        }
    }


    /// <summary>
    /// migrate old .data/app.xml to App_Data
    /// </summary>
    /// <param name="appRootPath"></param>
    public static void MigrateOldAppDataFile(string appRootPath)
    {
        var oldDataAppFilePath = Path.Combine(appRootPath, FolderConstants.DataFolderOld, FolderConstants.AppDataFile);
        if (!File.Exists(oldDataAppFilePath))
            return;

        Directory.CreateDirectory(Path.Combine(appRootPath, FolderConstants.DataFolderProtected));
        var newFilePath = Path.Combine(appRootPath, FolderConstants.DataFolderProtected, FolderConstants.AppDataFile);

        if (File.Exists(newFilePath))
            File.Delete(newFilePath);
        File.Move(oldDataAppFilePath, newFilePath);
    }

}