using System;

namespace ToSic.Eav.Run.Capabilities
{
    public class NetCoreCapability: SystemCapabilityBase, ISystemCapability
    {

        private static readonly SystemCapabilityDefinition DefStatic = new SystemCapabilityDefinition(
            "NetCore",
            new Guid("57c306d5-ec3f-47e2-ad3a-ae871eb96a41"),
            "Net Core"
        );

#if NETFRAMEWORK
        public NetCoreCapability(): base(DefStatic, false) { }
#else
        public NetCoreCapability(): base(DefStatic, true) { }
#endif

    }
}
