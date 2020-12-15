namespace ToSic.Eav.Apps
{
    public static class AppConstants
    {
        public const string AppIconFile = "app-icon.png";

        #region List / Content / Presentation capabilities

        // Known App Content Types
        //public const string TypeAppConfig = ImpExpConstants.TypeAppConfig; // "2SexyContent-App";
        //public const string TypeAppResources = "App-Resources";
        //public const string TypeAppSettings = "App-Settings";

        // App and Content Scopes
        public const string ScopeApp = "2SexyContent-App";
        public const string ScopeContentFuture = "Default";
        public const string ScopeContentOld = "2SexyContent";
        public static readonly string[] ScopesContent = {ScopeContentOld, ScopeContentFuture };

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
    }
}
