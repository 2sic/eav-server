using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    public static List<FeatureLicenseRule> ForForWebFarmCacheDisabled = BuildRule(BuiltInLicenses.WebFarmCache, false);
    public static List<FeatureLicenseRule> ForForWebFarmCacheEnabled = BuildRule(BuiltInLicenses.WebFarmCache, true);

    public static readonly Feature WebFarmCache = new()
    {
        NameId = "WebFarmCache",
        Guid = new("11c0fedf-16a7-4596-900c-59e860b47965"),
        Name = "Web Farm Cache",
        IsPublic = false,
        Ui = false,
        Description = "Enables Web Farm Cache use in Dnn", Security = FeaturesCatalogRules.Security0Improved,
        LicenseRules = ForForWebFarmCacheDisabled
    };

    public static readonly Feature WebFarmCacheDebug = new()
    {
        NameId = "WebFarmCacheDebug",
        Guid = new("031cf718-271e-41de-89ca-d1dd4ecfe602"),
        Name = "Web Farm Cache with verbose debugging",
        IsPublic = false,
        Ui = false,
        Description = "Enables Web Farm Cache use in Dnn with more debugging, to better find issues",
        Security = FeaturesCatalogRules.Security0Improved,
        LicenseRules = ForForWebFarmCacheDisabled
    };
}