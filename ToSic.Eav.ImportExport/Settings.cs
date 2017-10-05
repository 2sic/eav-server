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
            ".data"
        };

        public static readonly string TemporaryDirectory = "~/DesktopModules/ToSIC_SexyContent/_";
        public static readonly string ToSexyDirectory = "~/DesktopModules/ToSIC_SexyContent";
        public const string TemplateContentType = "2SexyContent-Template";
    }
}