using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public class FeatureLicenseRule
{
    public FeatureLicenseRule(FeatureSet featureSet, bool enabled)
    {
        FeatureSet = featureSet;
        EnableFeatureByDefault = enabled;
    }

    public FeatureSet FeatureSet { get; }

    public bool EnableFeatureByDefault { get; } = true;
}