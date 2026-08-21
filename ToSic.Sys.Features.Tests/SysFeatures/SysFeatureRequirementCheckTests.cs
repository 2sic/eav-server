using ToSic.Sys.Capabilities;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.SysFeatures;
using ToSic.Sys.Requirements;

namespace ToSic.Sys.Features.SysFeatures;

/// <summary>
/// Test the RequirementsService using the registered detectors.
/// </summary>
/// <param name="requirementsService"></param>
public class SysFeatureRequirementChecks(IRequirementsService requirementsService, ISysFeaturesService featuresSvc, SysFeaturesLoader sysFeatLoader)
{
    public class Startup() : QuickStartup(sc => sc.AddSysCapabilitiesAndSysCore());

    private void LoadSysFeaturesFromAllAssemblies()
    {
        featuresSvc.UpdateFeatureList(new(), sysFeatLoader.Load());
    }
    
    [Theory]
#if NETCOREAPP
    [InlineData(true)]
#else
    [InlineData(false)]
#endif
    public void Requirement_DotNetCore_MatchesTestRuntime(bool expectsIsNull)
    {
        LoadSysFeaturesFromAllAssemblies();
        
        var ok = requirementsService.CheckOneInternalTac(new(FeatureConstants.RequirementSysCapability, SysFeatureDetectorNetCore.DefStatic.NameId));
        Equal(expectsIsNull, ok == null);
    }

    [Theory]
#if NETCOREAPP
    [InlineData(false)]
#else
    [InlineData(true)]
#endif
    public void Requirement_DotNetFramework_MatchesTestRuntime(bool expectsIsNull)
    {
        LoadSysFeaturesFromAllAssemblies();

        var ok = requirementsService.CheckOneInternalTac(new(FeatureConstants.RequirementSysCapability, SysFeatureDetectorNetFramework.DefStatic.NameId));
        Equal(expectsIsNull, ok == null);
    }

}