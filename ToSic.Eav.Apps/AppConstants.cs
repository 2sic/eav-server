namespace ToSic.Eav.Apps
{
    public static class AppConstants
    {
        public const string AppIconFile = "app-icon.png";


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
        public static string AppPathPlaceholder = "[App:Path]";
    }
}
