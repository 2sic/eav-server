using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Testing;

namespace ToSic.Eav.DataSource.DbTests.VerifyFullDbStartUp;

[Startup(typeof(TestStartupFullWithDb))]
public class VerifyFullDbFixture(IDbConfiguration dbConfiguration) : IClassFixture<FullDbFixtureScenarioBasic>
{
    [Fact]
    public void VerifyFullDbFixtureConnectionString() =>
        Equal(EavTestConfig.ScenarioBasic.ConStr, dbConfiguration.ConnectionString);

}