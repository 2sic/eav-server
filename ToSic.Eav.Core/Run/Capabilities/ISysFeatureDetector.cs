using ToSic.Eav.SysData;

namespace ToSic.Eav.Run.Capabilities
{
    public interface ISysFeatureDetector
    {
        SystemCapabilityDefinition Definition { get; }

        bool IsEnabled { get; }

        FeatureState FeatState { get; }

    }
}
