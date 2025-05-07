using ToSic.Eav.Internal.Features;

namespace ToSic.Lib.Features.Tests.SysFeaturesTests;

public class DotNetSysFeatures
{
    [Theory]
#if NETCOREAPP
    [InlineData(true)]
#else
    [InlineData(false)]
#endif
    public void NetCoreIsOk(bool expected)
        => Equal(expected, new SysFeatureDetectorNetCore().IsEnabled);

    [Theory]
#if NETCOREAPP
    [InlineData(false)]
#else
    [InlineData(true)]
#endif
    public void NetFrameworkIsOk(bool expected)
        => Equal(expected, new SysFeatureDetectorNetFramework().IsEnabled);

}