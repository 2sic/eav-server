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
        public const string FolderData = ".data";

        /// <summary>
        /// Protected folder - IIS Request filtering default hidden segment
        /// contains the app.xml for export/import of the app
        /// contains the .app.json
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
        public const string DotAppJson = ".app.json";

        /// <summary>
        /// the .databeta (this is for testing only, will never be in the distribution)
        /// </summary>
        public const string FolderDataBeta = ".databeta";

        /// <summary>
        /// This is for data-customizations on global 2sxc/environment, which won't get replaced on updates
        /// </summary>
        public const string FolderDataCustom = ".data-custom";

        /// <summary>
        /// TemporaryFolder in the global environment
        /// </summary>
        public const string TemporaryFolder = "_";

        /// <summary>
        /// InstructionsFolder in the global environment
        /// </summary>
        public const string InstructionsFolder = "ImportExport\\Instructions";
    }
}
