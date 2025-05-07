using ToSic.Eav.Internal.Licenses;

namespace ToSic.Eav.Internal.Features;

public partial class BuiltInFeatures
{
    public static List<FeatureLicenseRule> ForPatronsPerfectionist = BuiltInLicenseRules.BuildRule(BuiltInLicenses.PatronPerfectionist, true);
    public static List<FeatureLicenseRule> ForPatronsSentinelDisabled = BuiltInLicenseRules.BuildRule(BuiltInLicenses.PatronSentinel, false);
    public static List<FeatureLicenseRule> ForPatronsSentinelEnabled = BuiltInLicenseRules.BuildRule(BuiltInLicenses.PatronSentinel, true);

}