using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features
{
    public abstract class SysFeatureDetector: ISysFeatureDetector
    {
        protected SysFeatureDetector(SystemCapabilityDefinition definition, bool isEnabled = default)
        {
            Definition = definition;
            IsEnabled = isEnabled;
        }

        public SystemCapabilityDefinition Definition { get; }

        public virtual bool IsEnabled { get; }

        public FeatureState FeatState => FeatureState.SysFeatureState(Definition, IsEnabled);
    }
}
