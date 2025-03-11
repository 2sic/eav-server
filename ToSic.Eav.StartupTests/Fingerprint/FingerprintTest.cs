using ToSic.Eav.Security.Fingerprint;
using ToSic.Eav.Testing;

namespace ToSic.Eav.Fingerprint;

[Startup(typeof(StartupTestFullWithDb))]
public class FingerprintTest(SystemFingerprint fingerprintSvc, SystemFingerprint fingerprintSvc2) : IClassFixture<FullDbFixtureScenarioBasic>
{
    [Fact]
    public void HasFingerprint() => NotNull(fingerprintSvc.GetFingerprint());

    [Fact]
    public void FingerprintIsLong() => True(fingerprintSvc.GetFingerprint().Length > 10);

    [Fact]
    public void FingerprintsMatch() => Equal(fingerprintSvc.GetFingerprint(), fingerprintSvc2.GetFingerprint());

}