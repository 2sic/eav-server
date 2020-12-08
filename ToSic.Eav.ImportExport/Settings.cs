namespace ToSic.Eav.ImportExport
{
    public class Settings
    {
        public static string FileVersion = "07.00.00";
        public static string MinimumRequiredVersion = "07.00.00";

        public static string[] ExcludeFolders =
        {
            ".git",
            "node_modules",
            "bower_components",
            ".vs",
        };

        public static string[] ExcludeRootFolders =
        {
            Constants.FolderData
        };

        public const string TemplateContentType = "2SexyContent-Template";
    }
}