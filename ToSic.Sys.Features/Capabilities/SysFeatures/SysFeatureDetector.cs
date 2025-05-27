using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public abstract class SysFeatureDetector(SysFeature definition, bool isEnabled = default) : ISysFeatureDetector
{
    public SysFeature Definition { get; } = definition;

    public virtual bool IsEnabled { get; } = isEnabled;

    public FeatureState FeatState => FeatureState.SysFeatureState(Definition, IsEnabled);
}