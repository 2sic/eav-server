namespace ToSic.Eav.Sys;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class FolderConstants
{
    /// <summary>
    /// New canonical folder inside an App-folder for installable extensions.
    /// </summary>
    public const string AppExtensionsFolder = "extensions";
    
    /// <summary>
    /// Folder inside an App-folder, containing extensions and other system features
    /// </summary>
    public const string AppExtensionsLegacyFolder = "system";

    /// <summary>
    /// File name for an extension configuration inside an Extension's App_Data folder
    /// </summary>
    public const string AppExtensionJsonFile = "extension.json";

    /// <summary>
    /// File name for an extension index/lock file inside an Extension's App_Data folder
    /// </summary>
    public const string AppExtensionLockJsonFile = "extension.lock.json";

    /// <summary>
    /// OLD Data folder - either in the global environment, in plugins or in app-extensions
    /// </summary>
    [PrivateApi("deprecated, no value in showing")]
    public const string DataFolderOld = ".data";

    public const string DataSubFolderSystem = "system";

    public const string DataSubFolderSystemBeta = "system-beta";

    public const string DataSubFolderSystemCustom = "system-custom";

    /// <summary>
    /// Protected folder - IIS Request filtering default hidden segment
    /// contains the app.xml for export/import of the app
    /// contains the app.json
    /// </summary>
    public const string DataFolderProtected = "App_Data";

    /// <summary>
    /// Folder in app export/import zip, that contains 2sxc application files
    /// </summary>
    public const string ToSxcFolder = "2sexy";

    /// <summary>
    /// app.xml for git-sync
    /// </summary>
    public const string AppDataFile = "app.xml";

    /// <summary>
    /// optional json file in App_Data folder with exclude configuration
    /// to define files and folders that will not be exported in app export
    /// </summary>
    public const string AppJsonFile = "app.json";

    /// <summary>
    /// TemporaryFolder in the global environment
    /// </summary>
    public const string TemporaryFolder = "_";

    /// <summary>
    /// InstructionsFolder in the global environment
    /// </summary>
    public const string InstructionsFolder = "ImportExport\\Instructions";

    /// <summary>
    /// Name of folder that should contain portal files in export zip or App_Data
    /// </summary>
    public const string ZipFolderForPortalFiles = "PortalFiles"; // used in v14

    public const string ZipFolderForSiteFiles = "SiteFiles"; // replace "PortalFiles" in v15

    /// <summary>
    /// Name of folder that should contain app files in export zip
    /// </summary>
    public const string ZipFolderForAppStuff = "2sexy";

    /// <summary>
    /// Name of folder that should contain global app files in export zip
    /// </summary>
    public const string ZipFolderForGlobalAppStuff = "2sexyGlobal";

    /// <summary>
    /// Name of folder for template for new app
    /// </summary>
    public const string NewAppFolder = "new-app";

    /// <summary>
    /// HotBuild AppCode folder
    /// </summary>
    public const string AppCodeFolder = "AppCode";

    /// <summary>
    /// Temp folder where the 2sxc app temp assemblies for AppCode, assembly dependencies... are stored.
    /// </summary>
    public const string TempAssemblyFolder = "2sxc.bin";

    /// <summary>
    /// Temp folder where the 2sxc app temp assemblies for Cshtml are stored.
    /// </summary>
    public const string CshtmlAssemblyFolder = "2sxc.bin.cshtml";

    /// <summary>
    /// Secure folder where RSA keys are stored
    /// </summary>
    public const string CryptoFolder = "2sxc.crypto";
}
