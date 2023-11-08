namespace ToSic.Eav.Run.Capabilities
{
    public interface ISystemCapability
    {
        SystemCapabilityDefinition Definition { get; }

        bool IsAvailable { get; }
    }
}
