using ToSic.Sys.Capabilities;
using ToSic.Sys.Capabilities.SysFeatures;
using ToSic.Sys.Requirements;

namespace ToSic.Sys.Features.SysFeatures;

/// <summary>
/// Test the RequirementsService using the registered detectors.
/// </summary>
/// <param name="requirementsService"></param>
public class SysFeatureRequirementChecks(RequirementsService requirementsService)
{
    public class Startup() : QuickStartup(sc => sc.AddSysCapabilitiesAndSysCore());

    [Theory]
#if NETCOREAPP
    [InlineData(true)]
#else
    [InlineData(false)]
#endif
    public void Requirement_DotNetCore_MatchesTestRuntime(bool expectsIsNull)
    {
        var ok = requirementsService.CheckTac(new Requirement(FeatureConstants.RequirementSysCapability, SysFeatureDetectorNetCore.DefStatic.NameId));
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
        var ok = requirementsService.CheckTac(new Requirement(FeatureConstants.RequirementSysCapability, SysFeatureDetectorNetFramework.DefStatic.NameId));
        Equal(expectsIsNull, ok == null);
    }

}