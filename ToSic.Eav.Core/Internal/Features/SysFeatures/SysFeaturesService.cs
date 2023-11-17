using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Plumbing;
using ToSic.Eav.SysData;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Run.Capabilities
{
    public class SysFeaturesService: ServiceBase
    {
        private readonly IServiceProvider _sp;

        public SysFeaturesService(IServiceProvider sp) : base("Eav.SysCap")
        {
            _sp = sp;
        }

        public List<SystemCapabilityDefinition> Definitions => _list ?? (_list = LoadCapabilities().Defs);
        private static List<SystemCapabilityDefinition> _list;

        public List<FeatureState> States => _listState ?? (_listState = LoadCapabilities().States);
        private static List<FeatureState> _listState;


        private (List<SystemCapabilityDefinition> Defs, List<FeatureState> States) LoadCapabilities()
        {
            var services = AssemblyHandling.FindInherited(typeof(ISysFeatureDetector));
            var objects = services.Select(s => _sp.Build<ISysFeatureDetector>(s));
            var definitions = objects.Select(isco => isco.Definition).ToList();
            var states = objects.Select(isco => isco.FeatState).ToList();
            return (definitions, states);
        }

        public bool IsEnabled(string capabilityKey)
        {
            var capability = States.FirstOrDefault(c => c.Aspect.NameId.EqualsInsensitive(capabilityKey));
            return capability?.IsEnabled == true;
        }

        public SystemCapabilityDefinition GetDef(string capabilityKey) 
            => Definitions.FirstOrDefault(c => c.NameId.EqualsInsensitive(capabilityKey));
    }
}
