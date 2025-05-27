namespace ToSic.Sys.Capabilities.Features;

public class FeatureLicenseRule
{
    public FeatureLicenseRule(FeatureSet.FeatureSet featureSet, bool enabled)
    {
        FeatureSet = featureSet;
        EnableFeatureByDefault = enabled;
    }

    public FeatureSet.FeatureSet FeatureSet { get; }

    public bool EnableFeatureByDefault { get; } = true;
}