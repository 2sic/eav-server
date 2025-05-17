using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Security.Fingerprint;
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
        base.SetupFixtureConfiguration(testScenario);
        //StartupGlobalFoldersAndFingerprint(testScenario);
    }

    //public void StartupGlobalFoldersAndFingerprint(TestScenario testScenario)
    //{
    //    globalConfig.GlobalFolder(testScenario.GlobalFolder);
    //    if (Directory.Exists(testScenario.GlobalDataCustomFolder))
    //        globalConfig.DataCustomFolder(testScenario.GlobalDataCustomFolder);

    //    // Try to reset some special static variables which may cary over through many tests
    //    SystemFingerprint.ResetForTest();
    //}

}
public class FixtureStartupNoDb(IGlobalConfiguration globalConfig)
{
    public virtual void SetupFixtureConfiguration(TestScenario testScenario)
    {
        globalConfig.GlobalFolder(testScenario.GlobalFolder);
        if (Directory.Exists(testScenario.GlobalDataCustomFolder))
            globalConfig.DataCustomFolder(testScenario.GlobalDataCustomFolder);

        // Try to reset some special static variables which may cary over through many tests
        SystemFingerprint.ResetForTest();
    }
}