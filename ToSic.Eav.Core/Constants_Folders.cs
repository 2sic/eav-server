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
        /// the .databeta (this is for testing only, will never be in the distribution)
        /// </summary>
        public const string FolderDataBeta = ".databeta";

        /// <summary>
        /// This is for data-customizations on global 2sxc/environment, which won't get replaced on updates
        /// </summary>
        public const string FolderDataCustom = ".data-custom";
    }
}
