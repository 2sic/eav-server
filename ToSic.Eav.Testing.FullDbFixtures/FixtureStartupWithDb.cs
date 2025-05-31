using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Testing.Scenarios;
using ToSic.Sys.Configuration;

namespace ToSic.Eav.Testing;

/// <summary>
/// Test-Helper to set up the database connection string and global folders.
/// It uses a TestScenario to set up everything.
/// </summary>
/// <param name="globalConfig"></param>
public class FixtureStartupWithDb(IGlobalConfiguration globalConfig)
    : FixtureStartupNoDb(globalConfig)
{

    public override void SetupFixtureConfiguration(TestScenario testScenario)
    {
        globalConfig.ConnectionString(testScenario.ConStr);
        base.SetupFixtureConfiguration(testScenario);
    }
}