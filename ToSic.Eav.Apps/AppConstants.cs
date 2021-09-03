namespace ToSic.Eav.Apps
{
    public static class AppConstants
    {
        public const string AppIconFile = "app-icon.png";

        #region List / Content / Presentation capabilities

        // App and Content Scopes
        public const string ScopeApp = "2SexyContent-App";
        public const string ScopeContentFuture = "Default";
        public const string ScopeContentOld = "2SexyContent";
        public static string ScopeConfiguration = "System.Configuration";
        public static readonly string[] ScopesContent = {ScopeContentOld, ScopeContentFuture };
        public static readonly string[] ScopesContentAndConfiguration = { ScopeContentOld, ScopeContentFuture, ScopeConfiguration };

        public const string ScopeContentSystem = "2SexyContent-System";

        #endregion

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
    }
}
