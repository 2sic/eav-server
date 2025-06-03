namespace ToSic.Eav.Apps;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppStackConstants
{
    #region Part Names inside the Stack

    public const string PartView = "ViewCustom";
    public const string PartViewSystem = "ViewSystem"; // not in use

    public const string PartApp = "AppCustom";
    public const string PartAppSystem = "AppSystem";

    public const string PartAncestor = "AncestorCustom";
    public const string PartAncestorSystem = "AncestorSystem";

    public const string PartSite = "SiteCustom";
    public const string PartSiteSystem = "SiteSystem";

    public const string PartGlobal = "GlobalCustom";
    public const string PartGlobalSystem = "GlobalSystem";

    public const string PartPreset = "PresetCustom"; // not in use
    public const string PartPresetSystem = "PresetSystem";

    #endregion

    // Settings / Resources
    public static string RootNameSettings = "Settings";
    public static string RootNameResources = "Resources";


    //public const string SysSettingsFieldScope = "SettingsEntityScope";

    //public static string FieldSettingsIdentifier = "SettingsIdentifier";
    //public static string FieldItemIdentifier = "ItemIdentifier";

    //internal static string[] BlacklistKeys = [FieldSettingsIdentifier, FieldItemIdentifier, SysSettingsFieldScope];

    public static AppThingsIdentifiers Resources = new()
    {
        AppType = AppLoadConstants.TypeAppResources,
        CustomType = "ResourcesCustom",
        SystemType = "ResourcesSystem",
        Target = AppThingsToStack.Resources
    };

    public static AppThingsIdentifiers Settings = new()
    {
        AppType = AppLoadConstants.TypeAppSettings,
        CustomType = "SettingsCustom",
        SystemType = "SettingsSystem",
        Target = AppThingsToStack.Settings
    };
}