using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features
{
    public interface ISysFeatureDetector
    {
        SystemCapabilityDefinition Definition { get; }

        bool IsEnabled { get; }

        FeatureState FeatState { get; }

    }
}
