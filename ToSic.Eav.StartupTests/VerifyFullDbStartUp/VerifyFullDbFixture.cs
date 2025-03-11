using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Testing;
using ToSic.Eav.Testing.Scenarios;

namespace ToSic.Eav.VerifyFullDbStartUp;

[Startup(typeof(StartupTestFullWithDb))]
public class VerifyFullDbFixture(IDbConfiguration dbConfiguration) : IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    [Fact]
    public void VerifyFullDbFixtureConnectionString() =>
        Equal(new ScenarioBasic().ConStr, dbConfiguration.ConnectionString);

}