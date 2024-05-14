using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    public static List<FeatureLicenseRule> ForForWebFarmCacheDisabled = BuildRule(BuiltInLicenses.WebFarmCache, false);
    public static List<FeatureLicenseRule> ForForWebFarmCacheEnabled = BuildRule(BuiltInLicenses.WebFarmCache, true);

    public static readonly Feature WebFarmCache = new(
        "WebFarmCache",
        new("11c0fedf-16a7-4596-900c-59e860b47965"),
        "Web Farm Cache",
        false,
        false,
        "Enables Web Farm Cache use in Dnn", FeaturesCatalogRules.Security0Improved,
        ForForWebFarmCacheDisabled
    );

    public static readonly Feature WebFarmCacheDebug = new(
        "WebFarmCacheDebug",
        new("031cf718-271e-41de-89ca-d1dd4ecfe602"),
        "Web Farm Cache with verbose debugging",
        false,
        false,
        "Enables Web Farm Cache use in Dnn with more debugging, to better find issues", FeaturesCatalogRules.Security0Improved,
        ForForWebFarmCacheDisabled
    );
}