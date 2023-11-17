#if DEBUG
using System;
using System.Collections.Generic;

namespace ToSic.Eav.Configuration
{
    public partial class BuiltInFeatures
    {
        public static List<FeatureLicenseRule> ForTestingDisabled = BuildRule(Licenses.BuiltInLicenses.CoreTesting, false);

        public static readonly FeatureDefinition TestingFeature001 = new FeatureDefinition(
            nameof(TestingFeature001),
            new Guid("f6cad4f7-f7ad-4205-9887-f7e28443ea8f"),
            "Just for testing!",
            false,
            false,
            "Just for testing!",
            FeaturesCatalogRules.Security0Improved,
            ForTestingDisabled
        );
    }
}
#endif
