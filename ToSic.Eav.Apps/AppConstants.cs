namespace ToSic.Eav.Apps
{
    public static class AppConstants
    {
        /// <summary>
        /// This is the folder name which contains all apps.
        /// As of now, it's hard-coded to be 2sxc
        /// If every EAV is used in other systems, we will have to put this into the global configuration and init in on startup
        /// </summary>
        public static string AppsRootFolder = "2sxc";

        public const string AppIconFile = "app-icon.png";
        public const string AppPrimaryIconFile = "app-primary.png";

        /// <summary>
        /// This is used in rare cases where the Zone should be auto-retrieved from the current context zone
        /// </summary>
        public const int AutoLookupZone = -1;

        public static readonly string ContentGroupRefTypeName = "ContentGroupReference";


        #region App Configuration Fields

        public const string
            FieldName = AppLoadConstants.FieldName,
            FieldFolder = AppLoadConstants.FieldName,
            FieldHidden = "Hidden";

        #endregion

        // this used to be a Settings.DataIsMissingInDb
        public const int AppIdNotFound = -100;

        // Settings / Resources
        public static string RootNameSettings = "Settings";
        public static string RootNameResources = "Resources";

        // Placeholders / Tokens - probably should move somewhere else someday
        // TODO: CHANGE TO [App:Folder] - must check UI code if this is used anywhere
        public const string AppFolderPlaceholder = "[AppFolder]";
        public static string AppPathPlaceholder = "[App:Path]";
        public static string AppPathSharedPlaceholder = "[App:PathShared]";
    }
}
