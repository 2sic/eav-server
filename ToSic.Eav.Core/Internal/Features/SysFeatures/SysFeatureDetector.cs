using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public abstract class SysFeatureDetector: ISysFeatureDetector
{
    protected SysFeatureDetector(SysFeature definition, bool isEnabled = default)
    {
        Definition = definition;
        IsEnabled = isEnabled;
    }

    public SysFeature Definition { get; }

    public virtual bool IsEnabled { get; }

    public FeatureState FeatState => FeatureState.SysFeatureState(Definition, IsEnabled);
}