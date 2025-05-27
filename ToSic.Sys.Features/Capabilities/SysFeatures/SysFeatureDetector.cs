using ToSic.Sys.Capabilities.Features;

namespace ToSic.Sys.Capabilities.SysFeatures;

public abstract class SysFeatureDetector(SysFeature definition, bool isEnabled = default) : ISysFeatureDetector
{
    public SysFeature Definition { get; } = definition;

    public virtual bool IsEnabled { get; } = isEnabled;

    public FeatureState FeatState => FeatureState.SysFeatureState(Definition, IsEnabled);
}