using ToSic.Eav.Testing.Scenarios;
using ToSic.Sys.Configuration;

namespace ToSic.Eav.Testing;

/// <summary>
/// Test-Helper to set up the database connection string and global folders.
/// It uses a TestScenario to set up everything.
/// </summary>
/// <param name="globalConfig"></param>
public class FixtureStartupWithDb(IGlobalConfiguration globalConfig)
#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
    : FixtureStartupNoDb(globalConfig)
#pragma warning restore CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
{

    public override void SetupFixtureConfiguration(TestScenario testScenario)
    {
        globalConfig.ConnectionString(testScenario.ConStr);
        base.SetupFixtureConfiguration(testScenario);
    }
}