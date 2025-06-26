using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;
using ToSic.Sys.Capabilities.FeatureSet;
using ToSic.Sys.Capabilities.Licenses;

namespace ToSic.Eav.Configuration;

[Startup(typeof(StartupTestsApps))]
public class LicenseCheckTest(ILicenseService licenseService)
    // the fixture will also load the licenses and stuff
    : IClassFixture<DoFixtureStartup<ScenarioFullPatronsWithDb>>
{
    /// <summary>
    /// Name of the license in the license.json file in Scenario Basic
    /// </summary>
    private const string TestLicenseName = "2sxc Dev - Dev License for 2dm, STV, SDV";
    private FeatureSetState GetLicenseOfTest() =>
        licenseService.All.First(l => l.Title == TestLicenseName);

    [Fact]
    public void LicenseExists() => NotNull(GetLicenseOfTest());

    [Fact]
    public void LicenseIsNotExpired() => True(GetLicenseOfTest().ExpirationIsValid);

    [Fact]
    public void FingerprintIsValid() => True(GetLicenseOfTest().FingerprintIsValid);

    [Fact]
    public void SignatureIsValid() => True(GetLicenseOfTest().SignatureIsValid);

    [Fact]
    public void VersionIsValid() => True(GetLicenseOfTest().VersionIsValid);

}