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

        public List<SystemCapabilityDefinition> Definitions => _list ?? (_list = LoadCapabilities().Defs);
        private static List<SystemCapabilityDefinition> _list;

        public List<SystemCapabilityState> States => _listState ?? (_listState = LoadCapabilities().States);
        private static List<SystemCapabilityState> _listState;


        private (List<SystemCapabilityDefinition> Defs, List<SystemCapabilityState> States) LoadCapabilities()
        {
            var services = AssemblyHandling.FindInherited(typeof(ISystemCapability));
            var objects = services.Select(s => _sp.Build<ISystemCapability>(s));
            var definitions = objects.Select(isco => isco.Definition).ToList();
            var states = objects.Select(isco => isco.State).ToList();
            return (definitions, states);
        }

        public bool IsEnabled(string capabilityKey)
        {
            var capability = States.FirstOrDefault(c => c.Definition.NameId.EqualsInsensitive(capabilityKey));
            return capability != null && capability.IsEnabled;
        }

        public SystemCapabilityDefinition GetDef(string capabilityKey) 
            => Definitions.FirstOrDefault(c => c.NameId.EqualsInsensitive(capabilityKey));
    }
}
