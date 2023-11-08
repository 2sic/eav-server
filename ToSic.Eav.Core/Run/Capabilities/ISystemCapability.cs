namespace ToSic.Eav.Run.Capabilities
{
    public interface ISystemCapability
    {
        SystemCapabilityDefinition Definition { get; }

        bool IsEnabled { get; }

        SystemCapabilityState State { get; }
    }
}
