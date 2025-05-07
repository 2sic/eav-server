using ToSic.Eav.Internal.Features;
using ToSic.Eav.Internal.Requirements;
using ToSic.Eav.SysData;
using ToSic.Lib.Features.Tests.RequirementsServiceTests;

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
        var ok = requirementsService.CheckTac(new Requirement(FeatureConstants.ConditionIsSysFeature, SysFeatureDetectorNetCore.DefStatic.NameId));
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
        var ok = requirementsService.CheckTac(new Requirement(FeatureConstants.ConditionIsSysFeature, SysFeatureDetectorNetFramework.DefStatic.NameId));
        Equal(expectsIsNull, ok == null);
    }

}