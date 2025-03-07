using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Security.Fingerprint;

namespace ToSic.Eav.Testing
{
    public class FullDbFixtureHelper(IDbConfiguration dbConfiguration, IGlobalConfiguration globalConfig)
    {

        public void Configure(EavTestConfig testConfig)
        {
            dbConfiguration.ConnectionString = testConfig.ConStr;

            StartupGlobalFoldersAndFingerprint(testConfig);
        }

        public void StartupGlobalFoldersAndFingerprint(EavTestConfig testConfig)
        {
            globalConfig.GlobalFolder = testConfig.GlobalFolder;
            var folderWithTestLicenses = testConfig.GlobalDataCustomFolder;
            if (Directory.Exists(folderWithTestLicenses))
                globalConfig.DataCustomFolder = folderWithTestLicenses;

            // Try to reset some special static variables which may cary over through many tests
            SystemFingerprint.ResetForTest();
        }

    }
}
