using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;
using ToSic.Sys.Configuration;

namespace ToSic.Eav.VerifyFullDbStartUp;

[Startup(typeof(StartupTestsApps))]
public class VerifyFullDbFixture(IGlobalConfiguration globalConfig) : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    [Fact]
    public void VerifyFullDbFixtureConnectionString() =>
        Equal(new ScenarioBasic().ConStr, globalConfig.ConnectionString());

}