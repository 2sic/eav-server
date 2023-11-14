namespace ToSic.Eav.Run.Capabilities
{
    public abstract class SystemCapability: ISystemCapability
    {
        protected SystemCapability(SystemCapabilityDefinition definition, bool isEnabled = default)
        {
            Definition = definition;
            IsEnabled = isEnabled;
        }

        public SystemCapabilityDefinition Definition { get; }

        public virtual bool IsEnabled { get; }

        public SystemCapabilityState State => new SystemCapabilityState(Definition, IsEnabled);
    }
}
