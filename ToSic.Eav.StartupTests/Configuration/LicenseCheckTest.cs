using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.SysData;
using ToSic.Eav.Testing;

namespace ToSic.Eav.Configuration;

[Startup(typeof(StartupTestFullWithDb))]
public class LicenseCheckTest(EavSystemLoader eavSystemLoader, ILicenseService licenseService) : IClassFixture<FullDbFixtureScenarioFullPatrons>
{
    /// <summary>
    /// Name of the license in the license.json file in Scenario Basic
    /// </summary>
    private const string TestLicenseName = "2sxc Dev - Dev License for 2dm, STV, SDV";

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

    private FeatureSetState GetLicenseOfTest()
    {
        eavSystemLoader.LoadLicenseAndFeatures();
        return licenseService.All.FirstOrDefault(l => l.Title == TestLicenseName);
    }
}