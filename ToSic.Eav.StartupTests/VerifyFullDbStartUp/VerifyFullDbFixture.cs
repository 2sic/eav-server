using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;

namespace ToSic.Eav.VerifyFullDbStartUp;

[Startup(typeof(StartupTestsApps))]
public class VerifyFullDbFixture(IDbConfiguration dbConfiguration, IGlobalConfiguration globalConfig) : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    [Fact]
    public void VerifyFullDbFixtureConnectionString() =>
        Equal(new ScenarioBasic().ConStr, globalConfig.ConnectionString());

}