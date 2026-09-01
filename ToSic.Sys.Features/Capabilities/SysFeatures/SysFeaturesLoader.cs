using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Utils.Assemblies;

namespace ToSic.Sys.Capabilities.SysFeatures;

/// <summary>
/// Loads all System Features through reflection, for use at start-up to expand the features service.
/// </summary>
/// <param name="sp"></param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public class SysFeaturesLoader(IServiceProvider sp)
    : ServiceBase("Eav.SysCap", connect: [/* never! sp*/ ])
{
    public IList<FeatureState> Load()
    {
        var services = AssemblyHandling.FindInherited(typeof(ISysFeatureDetector));

        var featDetectors = services
            .Select(s => sp.Build<ISysFeatureDetector>(s))
            .ToListOpt();

        var states = featDetectors
            .Select(fd => fd.GetState())
            .ToListOpt();

        return states;
    }
}