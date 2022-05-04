using System.Collections.Generic;
using ToSic.Eav.Configuration.Licenses;

namespace ToSic.Eav.Configuration
{
    public partial class BuiltInFeatures
    {
        // IMPORTANT
        // The guids of these licenses must match the ones in the 2sxc.org features list
        // So always create the definition there first, then use the GUID of that definition here


        internal static List<FeatureLicenseRule> BuildRule(LicenseDefinition licDef, bool enabled) => new List<FeatureLicenseRule>
        {
            new FeatureLicenseRule(licDef, enabled)
        };

    }
}
