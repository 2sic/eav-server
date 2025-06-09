using ToSic.Lib.DI;
using ToSic.Lib.Services;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Performance;
using ToSic.Sys.Utils;
using ToSic.Sys.Utils.Assemblies;

namespace ToSic.Sys.Capabilities.SysFeatures;

public class SysFeaturesService(IServiceProvider sp)
    : ServiceBase("Eav.SysCap", connect: [/* never! sp*/ ])
{
    public IList<SysFeature> Definitions => _list ??= LoadCapabilities().Defs;
    private static IList<SysFeature>? _list;

    public IList<FeatureState> States => _listState ??= LoadCapabilities().States;
    private static IList<FeatureState>? _listState;


    private (IList<SysFeature> Defs, IList<FeatureState> States) LoadCapabilities()
    {
        var services = AssemblyHandling.FindInherited(typeof(ISysFeatureDetector));

        var featDetectors = services
            .Select(s => sp.Build<ISysFeatureDetector>(s))
            .ToListOpt();

        var definitions = featDetectors
            .Select(fd => fd.Definition)
            .ToListOpt();

        var states = featDetectors
            .Select(fd => fd.FeatState)
            .ToListOpt();

        return (definitions, states);
    }

    public bool IsEnabled(string capabilityKey)
    {
        var capability = States
            .FirstOrDefault(c => c.Aspect.NameId.EqualsInsensitive(capabilityKey));
        return capability?.IsEnabled == true;
    }

    public SysFeature? GetDef(string capabilityKey) 
        => Definitions.FirstOrDefault(c => c.NameId.EqualsInsensitive(capabilityKey));
}