using System.Collections.Generic;
using ToSic.Eav.Configuration.Licenses;

namespace ToSic.Eav.Configuration
{
    public partial class FeaturesBuiltIn
    {

        internal static List<FeatureLicenseRule> BuildRule(LicenseDefinition licDef, bool enabled) => new List<FeatureLicenseRule>
        {
            new FeatureLicenseRule(licDef, enabled)
        };

    }
}
