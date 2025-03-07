using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Testing;
using ToSic.Testing;

namespace ToSic.Eav.DataSource.DbTests;

[Startup(typeof(TestStartupFullWithDb))]
public class VerifyFullDbFixture(IDbConfiguration dbConfiguration) : IClassFixture<FullDbFixtureScenarioBasic>
{
    [Fact]
    public void VerifyFullDbFixtureConstruction()
    {
        Equal(EavTestConfig.ScenarioBasic.ConStr, dbConfiguration.ConnectionString);
    }
}