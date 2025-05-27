using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;
using ToSic.Sys.Capabilities.Fingerprints;

namespace ToSic.Eav.Fingerprint;

[Startup(typeof(StartupTestsApps))]
public class FingerprintTest(SystemFingerprint fingerprintSvc, SystemFingerprint fingerprintSvc2) : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    [Fact]
    public void HasFingerprint() => NotNull(fingerprintSvc.GetFingerprint());

    [Fact]
    public void FingerprintIsLong() => True(fingerprintSvc.GetFingerprint().Length > 10);

    [Fact]
    public void FingerprintsMatch() => Equal(fingerprintSvc.GetFingerprint(), fingerprintSvc2.GetFingerprint());

}