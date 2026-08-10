using System.Xml.XPath;
using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Sys.State;
using ToSic.Eav.Data.Sys.Ancestors;
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
    : ServiceWithSetup<ZipExport.Options>(EavLogs.Eav + ".ZipExp",
        connect: [appReaders, xmlExporter, globalConfiguration, fileManagerGenerator, features])
{
    public record Options : IAppIdentity
    {
        public int ZoneId { get; init; }
        public int AppId { get; init; }
        public string AppFolder { get; init; } = "";
        public string PhysicalAppPath { get; init; } = "";
        public string PhysicalPathGlobal { get; init; } = "";
    }

    // 2026-08-07 2dm - believe that options are always required
    //protected override Options GetDefaultOptions() => new();


    private const string SexyContentContentGroupName = "2SexyContent-ContentGroup";
    private const string SourceControlDataFolder = FolderConstants.DataFolderProtected;
    private const string SourceControlDataFile = FolderConstants.AppDataFile;

    private AppFileManager AppFileManager => field
        ??= fileManagerGenerator.New().SetFolder(MyOptions.AppId, MyOptions.PhysicalAppPath);

    private AppFileManager AppFileManagerGlobal => field
        ??= fileManagerGenerator.New().SetFolder(MyOptions.AppId, MyOptions.PhysicalPathGlobal);

    public int CountFiles(bool withGlobal, Func<AppFileManager, IEnumerable<string>> countFn)
    {
        return countFn(AppFileManager).Count() + (withGlobal ? countFn(AppFileManagerGlobal).Count() : 0);
    }

    public PathCasePreflightResult PathCasePreflight(
        AppExportSpecs specs,
        IEnumerable<PathCaseItem> appReferences,
        IEnumerable<PathCaseItem> sharedAppReferences)
    {
        var l = Log.Fn<PathCasePreflightResult>(specs.Dump());
        var result = PathCasePreflight(GenerateExportXml(specs), appReferences, sharedAppReferences);
        return l.Return(result, $"{result.Issues.Count} issues");
    }

    private PathCasePreflightResult PathCasePreflight(
        XmlExporter xmlExport,
        IEnumerable<PathCaseItem> appReferences,
        IEnumerable<PathCaseItem> sharedAppReferences)
    {
        var l = Log.Fn<PathCasePreflightResult>();
        var validator = new PathCasePreflightValidator(l);
        var results = new List<PathCasePreflightResult>();

        results.Add(validator.Validate(
            PathCasePreflightValidator.ScopeAppFiles,
            appReferences,
            ActualAppPathsOrEmpty(AppFileManager, MyOptions.PhysicalAppPath)));

        results.Add(validator.Validate(
            PathCasePreflightValidator.ScopeSharedAppFiles,
            sharedAppReferences,
            ActualAppPathsOrEmpty(AppFileManagerGlobal, MyOptions.PhysicalPathGlobal)));

        var referencedFiles = xmlExport.ReferencedFiles
            .Where(file => file.RelativePath.HasValue())
            .ToList();
        results.Add(validator.Validate(
            PathCasePreflightValidator.ScopeDatabaseAssets,
            referencedFiles.Select(file => new PathCaseItem(file.RelativePath!)),
            referencedFiles.SelectMany(ActualReferencedFiles)));

        var result = new PathCasePreflightResult(results.SelectMany(item => item.Issues).ToList());
        result = validator.LogResult(result);
        return l.Return(result, $"{result.Issues.Count} issues");
    }

    public void ExportForSourceControl(AppExportSpecs specs)
    {
        var l = Log.Fn(specs.Dump());
        var appDataPath = Path.Combine(MyOptions.PhysicalAppPath, SourceControlDataFolder);
        l.A($"Target Path: {appDataPath}");

        // migrate old .data to App_Data also here
        // to ensure that older export is overwritten
        ZipImport.MigrateOldAppDataFile(MyOptions.PhysicalAppPath);

        // create App_Data unless exists
        Directory.CreateDirectory(appDataPath);

        // generate the XML & save
        var xmlExport = GenerateExportXml(specs);

        if (specs.WithSiteFiles)
        {
            l.A("Will include site files");
            var appDataDirectory = new DirectoryInfo(appDataPath);

            // 1. Copy app global templates folder for version control
            if (Directory.Exists(MyOptions.PhysicalPathGlobal))
            {
                // Sometimes delete is locked by external process
                try
                {
                    // Empty older version of app global templates state in App_Data
                    var globalTemplatesStatePath = Path.Combine(appDataPath, FolderConstants.ZipFolderForGlobalAppStuff);
                    Zipping.TryToDeleteDirectory(globalTemplatesStatePath, Log);
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
                Zipping.TryToDeleteDirectory(portalFilesPath, Log);

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
        l.A($"Generated XML for app #{specs.AppId}; Size: {xml.Length}");
        File.WriteAllText(Path.Combine(appDataPath, SourceControlDataFile), xml);
        l.Done();
    }

    public MemoryStream ExportApp(
        AppExportSpecs specs,
        IEnumerable<PathCaseItem> appReferences,
        IEnumerable<PathCaseItem> sharedAppReferences)
    {
        var l = Log.Fn<MemoryStream>(specs.Dump());

        // generate the XML
        var xmlExport = GenerateExportXml(specs);

        // This audit is informational and must never prevent an export.
        try
        {
            _ = PathCasePreflight(xmlExport, appReferences, sharedAppReferences);
        }
        catch (Exception e)
        {
            l.W("Path case preflight failed; export will continue");
            l.Ex(e);
        }

        // migrate old .data to App_Data also here
        // to ensure that older export is overwritten
        ZipImport.MigrateOldAppDataFile(MyOptions.PhysicalAppPath);

        #region Copy needed files to temporary directory

        var messages = new List<Message>();
        var randomShortFolderName = Guid.NewGuid().ToString().Substring(0, 4);

        var temporaryDirectoryPath = Path.Combine(globalConfiguration.TemporaryFolder(), randomShortFolderName);

        Directory.CreateDirectory(temporaryDirectoryPath); // create temp dir unless exists

        AddInstructionsToPackageFolder(temporaryDirectoryPath);

        var tempDirectory = new DirectoryInfo(temporaryDirectoryPath);
        var appDirectory = tempDirectory.CreateSubdirectory("Apps/" + MyOptions.AppFolder + "/");

        var sexyDirectory = appDirectory.CreateSubdirectory(FolderConstants.ZipFolderForAppStuff);
        var globalSexyDirectory = appDirectory.CreateSubdirectory(FolderConstants.ZipFolderForGlobalAppStuff);
        var siteFilesDirectory = appDirectory.CreateSubdirectory(FolderConstants.ZipFolderForPortalFiles);

        // Copy app folder
        if (Directory.Exists(MyOptions.PhysicalAppPath))
            AppFileManager.CopyAllFiles(sexyDirectory.FullName, false, messages);

        // Copy global app folder only for ParentApp
        var parentAppGuid = xmlExport.AppReader.GetParentCache()?.NameId;
        if (parentAppGuid == null || AppStateExtensions.AppGuidIsAPreset(parentAppGuid))
            if (Directory.Exists(MyOptions.PhysicalPathGlobal))
                AppFileManagerGlobal.CopyAllFiles(globalSexyDirectory.FullName, false, messages);

        // Copy SiteFiles
        CopyPortalFiles(xmlExport, siteFilesDirectory, specs.AssetsAdam, specs.AssetsSite);
        #endregion

        // create tmp App_Data unless exists
        var tmpAppDataProtectedFolder = Path.Combine(appDirectory.FullName, FolderConstants.ToSxcFolder, FolderConstants.DataFolderProtected);
        Directory.CreateDirectory(tmpAppDataProtectedFolder);

        // Save export xml
        var xml = xmlExport.GenerateNiceXml();
        File.WriteAllText(Path.Combine(tmpAppDataProtectedFolder, FolderConstants.AppDataFile), xml);

        // Zip directory and return as stream
        var stream = new Zipping(Log).ZipDirectoryIntoStream(tempDirectory.FullName);

        Zipping.TryToDeleteDirectory(temporaryDirectoryPath, Log);

        return l.Return(stream, $"{stream.Length} bytes");
    }

    private void CopyPortalFiles(XmlExporter xmlExport, DirectoryInfo siteFilesDirectory, bool assetsAdam, bool assetsSite)
    {
        if (!assetsAdam || !assetsSite)
            // Verify patron features if they are being used
            features.ThrowIfNotEnabled("To skip exporting site files, you must enable system features.", [BuiltInFeatures.AppExportAssetsAdvanced.Guid]);

        foreach (var file in xmlExport.ReferencedFiles)
        {
            var relPath = file.RelativePath ?? throw new NullReferenceException("File relative path is null, this should not happen in export.");
            var physicalRelativePath = relPath.ToSystemPath();
            var portalFilePath = Path.Combine(siteFilesDirectory.FullName, Path.GetDirectoryName(physicalRelativePath) ?? string.Empty);

            Directory.CreateDirectory(portalFilePath);

            if (!File.Exists(file.Path))
                continue;

            var fullPath = Path.Combine(siteFilesDirectory.FullName, physicalRelativePath);
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

    private static IEnumerable<PathCaseItem> ActualAppPathsOrEmpty(AppFileManager fileManager, string root)
        => !Directory.Exists(root)
            ? []
            : fileManager.GetAllTransferableFiles()
                .Select(path => new PathCaseItem(RelativeToRoot(root, path)))
                .Concat(fileManager.GetAllTransferableFolders()
                    .Select(path => new PathCaseItem(RelativeToRoot(root, path), IsFolder: true)));

    private static IEnumerable<PathCaseItem> ActualReferencedFiles(TenantFileItem file)
    {
        if (file.RelativePath == null)
            return [];

        var relativeParts = file.RelativePath.ForwardSlash()
            .Split(['/'], StringSplitOptions.RemoveEmptyEntries);
        return PathCasePreflightValidator.FindActualPaths(file.Path)
            .Select(actualPath => actualPath.ForwardSlash().Split(['/'], StringSplitOptions.RemoveEmptyEntries))
            .Where(actualParts => actualParts.Length >= relativeParts.Length)
            .Select(actualParts => new PathCaseItem(
                string.Join("/", actualParts.Skip(actualParts.Length - relativeParts.Length))));
    }

    private static string RelativeToRoot(string root, string path)
        => path.Substring(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Length)
            .TrimPrefixSlash()
            .ForwardSlash();


    private XmlExporter GenerateExportXml(AppExportSpecs specs)
    {
        var appReader = appReaders.Get(MyOptions);
        // Get Export XML
        var contentTypes = appReader.ContentTypes.OfScope(includeAttributeTypes: true);
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
            appReader
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

        var xmlExport = xmlExporter.Init(specs, appReader, true, contentTypeNames, entityIds);

        #region reset App Guid if necessary

        if (!specs.ResetAppGuid)
            return xmlExport;

        // Reset the AppGuid in the xml export, so it can be used for a new app which will also have a new guid on import
        var root = xmlExport.ExportXDocument; //.Root;
        var appGuid = root.XPathSelectElement("/SexyContent/Header/App")!.Attribute(XmlConstants.Guid)!;
        appGuid.Value = Guid.Empty.ToString();
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
