using ToSic.Sys.Capabilities.SysFeatures;

namespace ToSic.Sys.Features.SysFeatures;

/// <summary>
/// Test the standalone detectors.
/// </summary>
public class DotNetSysFeaturesDetectors
{
    public class Startup() : QuickStartup(sc => sc.AddSysCapabilitiesAndSysCore());

    [Theory]
#if NETCOREAPP
    [InlineData(true)]
#else
    [InlineData(false)]
#endif
    public void NetCoreDetector_MatchesTestRuntime(bool expected)
        => Equal(expected, new SysFeatureDetectorNetCore().IsEnabled);

    [Theory]
#if NETCOREAPP
    [InlineData(false)]
#else
    [InlineData(true)]
#endif
    public void NetFrameworkDetector_MatchesTestRuntime(bool expected)
        => Equal(expected, new SysFeatureDetectorNetFramework().IsEnabled);

}