using ToSic.Sys.Capabilities;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.SysFeatures;
using ToSic.Sys.Requirements;

namespace ToSic.Sys.Features.SysFeatures;

/// <summary>
/// Test the RequirementsService using the registered detectors.
/// </summary>
/// <param name="requirementsService"></param>
public class RequirementsServiceSysFeatures(IRequirementsService requirementsService, ISysFeaturesService featuresSvc, SysFeaturesLoader sysFeatLoader)
{
    public class Startup() : QuickStartup(sc => sc.AddSysCapabilitiesAndSysCore());

    private void LoadSysFeaturesFromAllAssemblies()
    {
        featuresSvc.UpdateFeatureList(new(), sysFeatLoader.Load());
    }

    // Specify the IsNetCore test value depending on the .net framework being used for testing.
#if NETCOREAPP
    private const bool IsRunningNetCore = true;
#else
    private const bool IsRunningNetCore = false;
#endif


    [Theory]
    [InlineData(IsRunningNetCore)]
    public void Requirement_DotNetCore_MatchesTestRuntime(bool noIssueExpected)
    {
        LoadSysFeaturesFromAllAssemblies();
        
        var ok = requirementsService.StatusInternalTac(new(FeatureConstants.RequirementSysCapability, SysFeatureDetectorNetCore.DefStatic.NameId));
        Equal(noIssueExpected, ok.IsOk);
    }

    [Theory]
    [InlineData(!IsRunningNetCore)]
    public void Requirement_DotNetFramework_MatchesTestRuntime(bool noIssueExpected)
    {
        LoadSysFeaturesFromAllAssemblies();

        var ok = requirementsService.StatusInternalTac(new(FeatureConstants.RequirementSysCapability, SysFeatureDetectorNetFramework.DefStatic.NameId));
        Equal(noIssueExpected, ok.IsOk);
    }

}