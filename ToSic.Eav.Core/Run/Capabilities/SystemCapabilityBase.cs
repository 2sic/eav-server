namespace ToSic.Eav.Run.Capabilities
{
    public abstract class SystemCapabilityBase: ISystemCapability
    {
        protected SystemCapabilityBase(SystemCapabilityDefinition definition, bool isEnabled = default)
        {
            Definition = definition;
            IsEnabled = isEnabled;
        }

        public SystemCapabilityDefinition Definition { get; }

        public virtual bool IsEnabled { get; }

        public SystemCapabilityState State => new SystemCapabilityState(Definition, IsEnabled);
    }
}
