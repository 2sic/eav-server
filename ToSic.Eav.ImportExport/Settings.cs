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
            Constants.FolderData
        };

        public static readonly string ModuleDirectory = "~/desktopmodules/tosic_sexycontent";
        public static readonly string TemporaryDirectory =  ModuleDirectory + "_";
        public const string TemplateContentType = "2SexyContent-Template";
    }
}