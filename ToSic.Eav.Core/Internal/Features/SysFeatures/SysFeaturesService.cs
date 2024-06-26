﻿using ToSic.Eav.Plumbing;
using ToSic.Eav.SysData;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Internal.Features;

public class SysFeaturesService(IServiceProvider sp) : ServiceBase("Eav.SysCap", connect: [/* never! sp*/ ])
{
    public List<SysFeature> Definitions => _list ??= LoadCapabilities().Defs;
    private static List<SysFeature> _list;

    public List<FeatureState> States => _listState ??= LoadCapabilities().States;
    private static List<FeatureState> _listState;


    private (List<SysFeature> Defs, List<FeatureState> States) LoadCapabilities()
    {
        var services = AssemblyHandling.FindInherited(typeof(ISysFeatureDetector));
        var objects = services.Select(s => sp.Build<ISysFeatureDetector>(s));
        var definitions = objects.Select(isco => isco.Definition).ToList();
        var states = objects.Select(isco => isco.FeatState).ToList();
        return (definitions, states);
    }

    public bool IsEnabled(string capabilityKey)
    {
        var capability = States.FirstOrDefault(c => c.Aspect.NameId.EqualsInsensitive(capabilityKey));
        return capability?.IsEnabled == true;
    }

    public SysFeature GetDef(string capabilityKey) 
        => Definitions.FirstOrDefault(c => c.NameId.EqualsInsensitive(capabilityKey));
}