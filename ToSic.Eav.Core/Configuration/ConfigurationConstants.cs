using System.Collections.Generic;
using ToSic.Eav.Apps;

namespace ToSic.Eav.Configuration
{
    public class ConfigurationConstants
    {
        public const string TypeSystemSettings = "SystemSettings";
        public const string TypeCustomSettings = "Settings";
        public const string TypeSystemResources = "SystemResources";
        public const string TypeCustomResources = "Resources";

        public const string SysSettingsFieldScope = "SettingsEntityScope";

        public static string FieldSettingsIdentifier = "SettingsIdentifier";
        public static string FieldItemIdentifier = "ItemIdentifier";

        internal static string[] BlacklistKeys = { FieldSettingsIdentifier, FieldItemIdentifier, SysSettingsFieldScope };


        public const string SysSettingsScopeValueSite = "site";

        public static AppThingsIdentifiers[] AppThingsArray = new[]
        {
            new AppThingsIdentifiers { AppType = AppLoadConstants.TypeAppResources, CustomType = TypeCustomResources, SystemType = TypeSystemResources, Target = AppThingsToStack.Resources},
            new AppThingsIdentifiers { AppType = AppLoadConstants.TypeAppSettings, CustomType = TypeCustomSettings, SystemType = TypeSystemSettings, Target = AppThingsToStack.Settings }
        };

        public static Dictionary<AppThingsToStack, AppThingsIdentifiers> AppThings =
            new Dictionary<AppThingsToStack, AppThingsIdentifiers>
            {
                {
                    AppThingsToStack.Resources,
                    new AppThingsIdentifiers { AppType = AppLoadConstants.TypeAppResources, CustomType = TypeCustomResources, SystemType = TypeSystemResources, Target = AppThingsToStack.Resources}
                },
                {
                    AppThingsToStack.Settings,
                    new AppThingsIdentifiers { AppType = AppLoadConstants.TypeAppSettings, CustomType = TypeCustomSettings, SystemType = TypeSystemSettings, Target = AppThingsToStack.Settings }
                },
            };
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
