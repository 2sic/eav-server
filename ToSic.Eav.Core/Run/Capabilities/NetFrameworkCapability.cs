using System;

namespace ToSic.Eav.Run.Capabilities
{
    public class NetFrameworkCapability: SystemCapabilityBase
    {

        public static SystemCapabilityDefinition DefStatic { get; } = new SystemCapabilityDefinition(
            "NetFramework",
            new Guid("ebe6418e-1932-46bb-864c-80eb906dd2d3"),
            "Dot Net Framework"
        );

#if NETFRAMEWORK
        public NetFrameworkCapability() : base(DefStatic, true) { }
#else
        public NetFrameworkCapability() : base(DefStatic, false) { }
#endif

    }
}
