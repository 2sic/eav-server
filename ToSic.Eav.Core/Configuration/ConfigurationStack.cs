using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    /// <summary>
    /// This has the constants which are used in the stack of settings/resources/configuration of the system
    /// </summary>
    [PrivateApi]
    public class ConfigurationStack
    {
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
    }
}
