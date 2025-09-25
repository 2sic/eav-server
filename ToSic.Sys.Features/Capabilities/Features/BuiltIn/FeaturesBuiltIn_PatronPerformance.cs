using ToSic.Sys.Capabilities.Licenses;

namespace ToSic.Sys.Capabilities.Features;

public partial class  BuiltInFeatures
{
    public static List<FeatureLicenseRule> ForPatronPerformanceAutoEnabled = BuiltInLicenseRules.BuildRule(BuiltInLicenses.PatronPerformance, true);

    public static List<FeatureLicenseRule> ForPatronPerformanceNotEnabled = BuiltInLicenseRules.BuildRule(BuiltInLicenses.PatronPerformance, false);
}