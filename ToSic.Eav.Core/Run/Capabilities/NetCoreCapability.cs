using System;

namespace ToSic.Eav.Run.Capabilities
{
    public class NetCoreCapability: ISystemCapability
    {
        public SystemCapabilityDefinition Definition => DefStatic;

        private static readonly SystemCapabilityDefinition DefStatic = new SystemCapabilityDefinition(
            "NetCore",
            new Guid("57c306d5-ec3f-47e2-ad3a-ae871eb96a41"),
            "Net Core"
        );

#if NETFRAMEWORK
        public bool IsAvailable => false;
#else
        public bool IsAvailable => true;
#endif

    }
}
