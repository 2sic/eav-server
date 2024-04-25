namespace ToSic.Eav.ImportExport.Internal;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class Settings
{
    public static string FileVersion = "07.00.00";
    public static string MinimumRequiredDnnVersion = "07.04.02";

    public static string[] ExcludeFolders =
    [
        ".git",
        "node_modules",
        "bower_components",
        ".vs",
        "obj"
    ];

    public static string[] ExcludeRootFolders =
    [
        Constants.FolderOldDotData, // ".data" should be migrated to new location "App_Data/system", so no need for export for old ".data"
        $"{Constants.AppDataProtectedFolder}\\{Constants.ZipFolderForSiteFiles}",
        $"{Constants.AppDataProtectedFolder}\\{Constants.ZipFolderForGlobalAppStuff}"
    ];

    public const string TemplateContentType = "2SexyContent-Template";
    
}