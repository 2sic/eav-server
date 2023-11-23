#if DEBUG
using System;
using System.Collections.Generic;
using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features
{
    public partial class BuiltInFeatures
    {
        public static List<FeatureLicenseRule> ForTestingDisabled = BuildRule(BuiltInLicenses.CoreTesting, false);

        public static readonly Feature TestingFeature001 = new(
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
