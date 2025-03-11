using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Eav.Testing.Scenarios;

namespace ToSic.Eav.Testing
{
    public class FullDbFixtureHelper(IDbConfiguration dbConfiguration, IGlobalConfiguration globalConfig)
    {

        public void Configure(TestScenario testScenario)
        {
            dbConfiguration.ConnectionString = testScenario.ConStr;

            StartupGlobalFoldersAndFingerprint(testScenario);
        }

        public void StartupGlobalFoldersAndFingerprint(TestScenario testScenario)
        {
            globalConfig.GlobalFolder = testScenario.GlobalFolder;
            if (Directory.Exists(testScenario.GlobalDataCustomFolder))
                globalConfig.DataCustomFolder = testScenario.GlobalDataCustomFolder;

            // Try to reset some special static variables which may cary over through many tests
            SystemFingerprint.ResetForTest();
        }

    }
}
