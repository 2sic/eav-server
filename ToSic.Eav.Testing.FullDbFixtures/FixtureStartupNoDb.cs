using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Sys.Configuration;
using ToSic.Eav.Testing.Scenarios;
using ToSic.Sys.Capabilities.Fingerprints;
using ToSic.Sys.Configuration;

namespace ToSic.Eav.Testing;

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