using System;

namespace ToSic.Eav.Run.Capabilities
{
    public class SystemCapabilityNetFramework: SystemCapability
    {

        public static SystemCapabilityDefinition DefStatic { get; } = new SystemCapabilityDefinition(
            "NetFramework",
            new Guid("ebe6418e-1932-46bb-864c-80eb906dd2d3"),
            "Dot Net Framework"
        );

#if NETFRAMEWORK
        public SystemCapabilityNetFramework() : base(DefStatic, true) { }
#else
        public SystemCapabilityNetFramework() : base(DefStatic, false) { }
#endif

    }
}
