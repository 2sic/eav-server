using System.Collections.Generic;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features
{
    public partial class BuiltInFeatures
    {
        // IMPORTANT
        // The guids of these licenses must match the ones in the 2sxc.org features list
        // So always create the definition there first, then use the GUID of that definition here


        internal static List<FeatureLicenseRule> BuildRule(FeatureSet licDef, bool featureEnabled) => new()
        {
            new FeatureLicenseRule(licDef, featureEnabled)
        };

    }
}
