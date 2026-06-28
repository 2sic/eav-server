using ToSic.Sys.Capabilities.Features;

namespace ToSic.Sys.Capabilities.SysFeatures;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ISysFeatureDetector
{
    SysFeature Definition { get; }

    bool IsEnabled { get; }

    FeatureState FeatState { get; }

}