using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Plumbing;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Run.Capabilities
{
    public class SystemCapabilitiesServices: ServiceBase
    {
        private readonly IServiceProvider _sp;

        public SystemCapabilitiesServices(IServiceProvider sp) : base("Eav.SysCap")
        {
            _sp = sp;
        }

        public List<SystemCapabilityDefinition> All => _list ?? (_list = LoadCapabilities());
        private List<SystemCapabilityDefinition> _list;


        private List<SystemCapabilityDefinition> LoadCapabilities()
        {
            var services = AssemblyHandling.FindInherited(typeof(ISystemCapability));
            var objects = services.Select(s => _sp.Build<ISystemCapability>(s));
            return objects.Select(isco => isco.Definition).ToList();
        }
    }
}
