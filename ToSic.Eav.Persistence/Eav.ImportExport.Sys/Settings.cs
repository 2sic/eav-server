using ToSic.Eav.Sys;

namespace ToSic.Eav.ImportExport.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class Settings
{
    public static string FileVersion = "07.00.00";
    public static string MinimumRequiredDnnVersion = "07.04.02";

    public static string[] ExcludeFolders =
    [
        ".git",                 // Git versioning folder
        "node_modules",         // js dependencies, can always be restored with npm ci
        "bower_components",     // older js build dependencies
        ".vs",                  // Visual Studio settings / caches
        "obj",                  // temporary c# build files
        ".angular",             // temporary angular build files
    ];

    public static string[] ExcludeRootFolders =
    [
        FolderConstants.FolderOldDotData, // ".data" should be migrated to new location "App_Data/system", so no need for export for old ".data"
        $"{FolderConstants.AppDataProtectedFolder}\\{FolderConstants.ZipFolderForSiteFiles}",
        $"{FolderConstants.AppDataProtectedFolder}\\{FolderConstants.ZipFolderForGlobalAppStuff}"
    ];

    public const string TemplateContentType = "2SexyContent-Template";
    
}