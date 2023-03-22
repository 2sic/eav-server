using System.Collections.Generic;

namespace ToSic.Eav.Configuration
{
    public partial class BuiltInFeatures
    {
        public static List<FeatureLicenseRule> ForPatronsPerfectionist = BuildRule(Licenses.BuiltInLicenses.PatronPerfectionist, true);
        public static List<FeatureLicenseRule> ForPatronsSentinelDisabled = BuildRule(Licenses.BuiltInLicenses.PatronSentinel, false);
        public static List<FeatureLicenseRule> ForPatronsSentinelEnabled = BuildRule(Licenses.BuiltInLicenses.PatronSentinel, true);

    }
}
