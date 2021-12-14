using ToSic.Eav.Apps;

namespace ToSic.Eav.Configuration
{
    public class ConfigurationConstants
    {
        public const string SysSettingsFieldScope = "SettingsEntityScope";

        public static string FieldSettingsIdentifier = "SettingsIdentifier";
        public static string FieldItemIdentifier = "ItemIdentifier";

        internal static string[] BlacklistKeys = { FieldSettingsIdentifier, FieldItemIdentifier, SysSettingsFieldScope };


        //public const string SysSettingsScopeValueSite = "site";

        public static AppThingsIdentifiers Resources = new AppThingsIdentifiers
        {
            AppType = AppLoadConstants.TypeAppResources, 
            CustomType = "ResourcesCustom",
            SystemType = "ResourcesSystem",
            Target = AppThingsToStack.Resources
        };
        public static AppThingsIdentifiers Settings = new AppThingsIdentifiers
        {
            AppType = AppLoadConstants.TypeAppSettings, 
            CustomType = "SettingsCustom",
            SystemType = "SettingsSystem",
            Target = AppThingsToStack.Settings
        };

        public static AppThingsIdentifiers[] AppThingsArray =
        {
            Resources,
            Settings
        };

        public static string WebResourcesNode = "WebResources";
        public static string WebResourceEnabledField = "Enabled";
        public static string WebResourceHtmlField = "Html";

    }

    public enum AppThingsToStack
    {
        Resources,
        Settings
    }

    public struct AppThingsIdentifiers
    {
        public AppThingsToStack Target;
        public string SystemType;
        public string CustomType;
        public string AppType;
    }
}
