using System.Collections.Generic;
using ToSic.Eav.Internal.Licenses;

namespace ToSic.Eav.Internal.Features
{
    public partial class BuiltInFeatures
    {
        public static List<FeatureLicenseRule> ForPatronsPerfectionist = BuildRule(BuiltInLicenses.PatronPerfectionist, true);
        public static List<FeatureLicenseRule> ForPatronsSentinelDisabled = BuildRule(BuiltInLicenses.PatronSentinel, false);
        public static List<FeatureLicenseRule> ForPatronsSentinelEnabled = BuildRule(BuiltInLicenses.PatronSentinel, true);

    }
}
