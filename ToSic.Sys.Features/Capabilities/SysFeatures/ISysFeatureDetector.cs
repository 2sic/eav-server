using ToSic.Sys.Capabilities.Features;

namespace ToSic.Sys.Capabilities.SysFeatures;

public interface ISysFeatureDetector
{
    SysFeature Definition { get; }

    bool IsEnabled { get; }

    FeatureState FeatState { get; }

}