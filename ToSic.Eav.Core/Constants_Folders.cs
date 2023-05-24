namespace ToSic.Eav
{
    public partial class Constants
    {
        /// <summary>
        /// Folder inside an App-folder, containing extensions and other system features
        /// </summary>
        public const string FolderAppExtensions = "system";

        /// <summary>
        /// Data folder - either in the global environment, in plugins or in app-extensions
        /// </summary>
        public const string FolderOldDotData = ".data";
        public const string FolderSystem = "system"; // ex: ".data";

        /// <summary>
        /// the .databeta (this is for testing only, will never be in the distribution)
        /// </summary>
        public const string FolderOldDotDataBeta = ".databeta";
        public const string FolderSystemBeta = "system-beta"; // ex. ".databeta";

        /// <summary>
        /// This is for data-customizations on global 2sxc/environment, which won't get replaced on updates
        /// </summary>
        public const string FolderOldDotDataCustom = ".data-custom";
        public const string FolderSystemCustom = "system-custom"; // ".data-custom";

        /// <summary>
        /// Protected folder - IIS Request filtering default hidden segment
        /// contains the app.xml for export/import of the app
        /// contains the app.json
        /// </summary>
        public const string AppDataProtectedFolder = "App_Data";

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
        public const string AppJson = "app.json";

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
    }
}
