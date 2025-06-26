using ToSic.Lib.Features.Tests.RequirementsServiceTests;
using ToSic.Sys.Capabilities;
using ToSic.Sys.Capabilities.SysFeatures;
using ToSic.Sys.Requirements;

namespace ToSic.Lib.Features.Tests.SysFeaturesTests;

public class SysFeatureRequirementChecks(RequirementsService requirementsService)
{
    [Theory]
#if NETCOREAPP
    [InlineData(true)]
#else
    [InlineData(false)]
#endif
    public void TestDotNetCore(bool expectsIsNull)
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
    public void TestDotNetFramework(bool expectsIsNull)
    {
        var ok = requirementsService.CheckTac(new Requirement(FeatureConstants.RequirementSysCapability, SysFeatureDetectorNetFramework.DefStatic.NameId));
        Equal(expectsIsNull, ok == null);
    }

}