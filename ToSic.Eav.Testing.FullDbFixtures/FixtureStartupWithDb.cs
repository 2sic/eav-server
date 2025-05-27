using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Testing.Scenarios;
using ToSic.Lib.DI;

namespace ToSic.Eav.Testing;

/// <summary>
/// Test-Helper to set up the database connection string and global folders.
/// It uses a TestScenario to set up everything.
/// </summary>
/// <param name="dbConfiguration">Db Config - but lazy, for tests which don't use the DB</param>
/// <param name="globalConfig"></param>
public class FixtureStartupWithDb(LazySvc<IDbConfiguration> dbConfiguration, IGlobalConfiguration globalConfig)
    : FixtureStartupNoDb(globalConfig)
{

    public override void SetupFixtureConfiguration(TestScenario testScenario)
    {
        dbConfiguration.Value.ConnectionString = testScenario.ConStr;
        globalConfig.ConnectionString(testScenario.ConStr);
        base.SetupFixtureConfiguration(testScenario);
    }
}