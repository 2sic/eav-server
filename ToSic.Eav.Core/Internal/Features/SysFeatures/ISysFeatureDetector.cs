using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features
{
    public interface ISysFeatureDetector
    {
        SysFeature Definition { get; }

        bool IsEnabled { get; }

        FeatureState FeatState { get; }

    }
}
