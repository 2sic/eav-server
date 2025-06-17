﻿using System.Xml.XPath;
using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.State;
using ToSic.Eav.Data.Ancestors.Sys;
using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Eav.ImportExport.Sys.Xml;
using ToSic.Eav.ImportExport.Sys.XmlExport;
using ToSic.Eav.Persistence.Sys.Logging;
using ToSic.Eav.Sys;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.SysFeatures;
using ToSic.Sys.Configuration;

namespace ToSic.Eav.ImportExport.Sys.Zip;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ZipExport(
    IAppReaderFactory appReaders,
    XmlExporter xmlExporter,
    Generator<AppFileManager> fileManagerGenerator,
    IGlobalConfiguration globalConfiguration,
    ISysFeaturesService features
    )
    : ServiceBase(EavLogs.Eav + ".ZipExp",
        connect: [appReaders, xmlExporter, globalConfiguration, fileManagerGenerator, features])
{
    private int _appId;
    private int _zoneId;
    private const string SexyContentContentGroupName = "2SexyContent-ContentGroup";
    private const string SourceControlDataFolder = FolderConstants.AppDataProtectedFolder;
    private const string SourceControlDataFile = FolderConstants.AppDataFile;
    private readonly string _blankGuid = Guid.Empty.ToString();

    public AppFileManager AppFileManager = null!;
    private string _physicalAppPath = null!;
    private string _appFolder = null!;

    public AppFileManager AppFileManagerGlobal = null!;
    private string _physicalPathGlobal = null!;

    #region DI Constructor

    public ZipExport Init(int zoneId, int appId, string appFolder, string physicalAppPath, string physicalPathGlobal)
    {
        _appId = appId;
        _zoneId = zoneId;
        _appFolder = appFolder;
        _physicalAppPath = physicalAppPath;
        _physicalPathGlobal = physicalPathGlobal;

        AppFileManager = fileManagerGenerator.New().SetFolder(appId, _physicalAppPath);
        AppFileManagerGlobal = fileManagerGenerator.New().SetFolder(appId, physicalPathGlobal);
        
        var appIdentity = new AppIdentity(_zoneId, _appId);
        _appReader = appReaders.Get(appIdentity);
        return this;
    }

    private IAppReader _appReader = null!;
    #endregion

    public void ExportForSourceControl(AppExportSpecs specs)
    {
        var appDataPath = Path.Combine(_physicalAppPath, SourceControlDataFolder);

        // migrate old .data to App_Data also here
        // to ensure that older export is overwritten
        ZipImport.MigrateOldAppDataFile(_physicalAppPath);

        // create App_Data unless exists
        Directory.CreateDirectory(appDataPath);

        // generate the XML & save
        var xmlExport = GenerateExportXml(specs);

        if (specs.WithSiteFiles)
        {
            var appDataDirectory = new DirectoryInfo(appDataPath);

            // 1. Copy app global templates folder for version control
            if (Directory.Exists(_physicalPathGlobal))
            {
                // Sometimes delete is locked by external process
                try
                {
                    // Empty older version of app global templates state in App_Data
                    var globalTemplatesStatePath = Path.Combine(appDataPath, FolderConstants.ZipFolderForGlobalAppStuff);
                    ZipImport.TryToDeleteDirectory(globalTemplatesStatePath, Log);
                    // Version control folder to preserve copy of app global templates
                    var globalTemplatesStateFolder = appDataDirectory.CreateSubdirectory(FolderConstants.ZipFolderForGlobalAppStuff);

                    // Copy app global templates for version control
                    var _ = new List<Message>();
                    AppFileManagerGlobal.CopyAllFiles(globalTemplatesStateFolder.FullName, true, _);
                }
                catch (Exception e)
                {
                    Log.Ex(e);
                }
            }

            // 2. Copy SiteFiles for version control
            try
            {
                // Empty older version of SiteFiles state in App_Data
                var portalFilesPath = Path.Combine(appDataPath, FolderConstants.ZipFolderForSiteFiles);
                ZipImport.TryToDeleteDirectory(portalFilesPath, Log);

                // Version control folder to preserve copy of SiteFiles
                var portalFilesDirectory = appDataDirectory.CreateSubdirectory(FolderConstants.ZipFolderForSiteFiles);

                // Copy SiteFiles for version control
                CopyPortalFiles(xmlExport, portalFilesDirectory, specs.AssetsAdam, specs.AssetsSite);
            }
            catch (Exception e)
            {
                Log.Ex(e);
            }
        }
        else
            // Verify patron features if they are being used
            if (specs.ResetAppGuid)
                features.ThrowIfNotEnabled("To skip exporting site files, you must enable system features.", [BuiltInFeatures.AppExportAssetsAdvanced.Guid]);

        var xml = xmlExport.GenerateNiceXml();
        File.WriteAllText(Path.Combine(appDataPath, SourceControlDataFile), xml);
    }

    public MemoryStream ExportApp(AppExportSpecs specs)
    {
        // generate the XML
        var xmlExport = GenerateExportXml(specs);

        // migrate old .data to App_Data also here
        // to ensure that older export is overwritten
        ZipImport.MigrateOldAppDataFile(_physicalAppPath);

        #region Copy needed files to temporary directory

        var messages = new List<Message>();
        var randomShortFolderName = Guid.NewGuid().ToString().Substring(0, 4);

        var temporaryDirectoryPath = Path.Combine(globalConfiguration.TemporaryFolder(), randomShortFolderName);

        Directory.CreateDirectory(temporaryDirectoryPath); // create temp dir unless exists

        AddInstructionsToPackageFolder(temporaryDirectoryPath);

        var tempDirectory = new DirectoryInfo(temporaryDirectoryPath);
        var appDirectory = tempDirectory.CreateSubdirectory("Apps/" + _appFolder + "/");

        var sexyDirectory = appDirectory.CreateSubdirectory(FolderConstants.ZipFolderForAppStuff);
        var globalSexyDirectory = appDirectory.CreateSubdirectory(FolderConstants.ZipFolderForGlobalAppStuff);
        var siteFilesDirectory = appDirectory.CreateSubdirectory(FolderConstants.ZipFolderForPortalFiles);

        // Copy app folder
        if (Directory.Exists(_physicalAppPath))
            AppFileManager.CopyAllFiles(sexyDirectory.FullName, false, messages);

        // Copy global app folder only for ParentApp
        var parentAppGuid = xmlExport.AppReader.GetParentCache()?.NameId;
        if (parentAppGuid == null || AppStateExtensions.AppGuidIsAPreset(parentAppGuid))
            if (Directory.Exists(_physicalPathGlobal))
                AppFileManagerGlobal.CopyAllFiles(globalSexyDirectory.FullName, false, messages);

        // Copy SiteFiles
        CopyPortalFiles(xmlExport, siteFilesDirectory, specs.AssetsAdam, specs.AssetsSite);
        #endregion

        // create tmp App_Data unless exists
        var tmpAppDataProtectedFolder = Path.Combine(appDirectory.FullName, FolderConstants.ToSxcFolder, FolderConstants.AppDataProtectedFolder);
        Directory.CreateDirectory(tmpAppDataProtectedFolder);

        // Save export xml
        var xml = xmlExport.GenerateNiceXml();
        File.WriteAllText(Path.Combine(tmpAppDataProtectedFolder, FolderConstants.AppDataFile), xml);

        // Zip directory and return as stream
        var stream = new Zipping(Log).ZipDirectoryIntoStream(tempDirectory.FullName + "\\");

        ZipImport.TryToDeleteDirectory(temporaryDirectoryPath, Log);

        return stream;
    }

    private void CopyPortalFiles(XmlExporter xmlExport, DirectoryInfo siteFilesDirectory, bool assetsAdam, bool assetsSite)
    {
        if (!assetsAdam || !assetsSite)
            // Verify patron features if they are being used
            features.ThrowIfNotEnabled("To skip exporting site files, you must enable system features.", [BuiltInFeatures.AppExportAssetsAdvanced.Guid]);

        foreach (var file in xmlExport.ReferencedFiles)
        {
            var relPath = file.RelativePath ?? throw new NullReferenceException("File relative path is null, this should not happen in export.");
            var portalFilePath = Path.Combine(siteFilesDirectory.FullName, Path.GetDirectoryName(relPath)!);

            Directory.CreateDirectory(portalFilePath);

            if (!File.Exists(file.Path))
                continue;

            var fullPath = Path.Combine(siteFilesDirectory.FullName, relPath);
            try
            {
                var pathStartWithAdam = relPath.StartsWith("adam");
                if (assetsAdam && pathStartWithAdam // Adam assets
                    || assetsSite && !pathStartWithAdam) // Site assets
                    File.Copy(file.Path!, fullPath, overwrite: true);
            }
            catch (Exception e)
            {
                throw new("Error on " + fullPath + " (" + fullPath.Length + ")", e);
            }
        }
    }


    private XmlExporter GenerateExportXml(AppExportSpecs specs)
    {
            // Get Export XML
        var appIdentity = new AppIdentity(_zoneId, _appId);
        var contentTypes = _appReader.ContentTypes.OfScope(includeAttributeTypes: true);
        contentTypes = contentTypes
            .Where(a => !((a as IContentTypeShared)?.AlwaysShareConfiguration ?? false));

        // Exclude ParentApp attributeSets
        // TODO: option to include ParentApp attributeSets
        contentTypes = contentTypes
            .Where(p => !p.HasAncestor());

        var contentTypeNames = contentTypes
            .Select(p => p.NameId)
            .ToArray();

        // 2022-01-04 2dm - new code, simplified
        // Get all entities except Attribute/Field Metadata, which is exported in a different way
        var entities =
            //dataSourceServices
            //.CreateDefault(new DataSourceOptions
            //{
            //    AppIdentityOrReader = appIdentity,
            //    ShowDrafts = true,
            //})
            _appReader
            .List
            .Where(e => e.MetadataFor.TargetType != (int)TargetTypes.Attribute)
            .ToList();

        if (!specs.IncludeContentGroups)
            entities = entities
                .Where(p => p.Type.NameId != SexyContentContentGroupName)
                .ToList();

        // Exclude ParentApp entities
        // TODO: option to include ParentApp entities
        entities = entities
            .Where(p => !p.HasAncestor())
            .ToList();

        var entityIds = entities
            .Select(e => e.EntityId.ToString())
            .ToArray();

        var xmlExport = xmlExporter.Init(specs, _appReader, true, contentTypeNames, entityIds);

        #region reset App Guid if necessary

        if (!specs.ResetAppGuid)
            return xmlExport;

        // Reset the AppGuid in the xml export, so it can be used for a new app which will also have a new guid on import
        var root = xmlExport.ExportXDocument; //.Root;
        var appGuid = root.XPathSelectElement("/SexyContent/Header/App")!.Attribute(XmlConstants.Guid)!;
        appGuid.Value = _blankGuid;
        return xmlExport;
        #endregion
    }

    /// <summary>
    /// This adds various files to an app-package, so anybody who gets such a package
    /// is informed as to what they must do with it.
    /// </summary>
    /// <param name="targetPath"></param>
    private void AddInstructionsToPackageFolder(string targetPath)
    {
        var srcPath = globalConfiguration.InstructionsFolder();

        foreach (var file in Directory.GetFiles(srcPath))
            File.Copy(file, Path.Combine(targetPath, Path.GetFileName(file)));
    }
}