using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Lib.DI;
using ToSic.Testing.Shared;

namespace ToSic.Testing
{
    public class FullDbFixtureHelper(IServiceProvider serviceProvider)
    {
        private T GetService<T>() where T : class => serviceProvider.Build<T>(/*Log*/);

        public void Configure(EavTestConfig testConfig)
        {
            var dbConfiguration = GetService<IDbConfiguration>();
            dbConfiguration.ConnectionString = testConfig.ConStr;

            StartupGlobalFoldersAndFingerprint(testConfig);
        }

        public void StartupGlobalFoldersAndFingerprint(EavTestConfig testConfig)
        {
            var globalConfig = GetService<IGlobalConfiguration>();
            globalConfig.GlobalFolder = testConfig.GlobalFolder;
            var folderWithTestLicenses = testConfig.GlobalDataCustomFolder;
            if (Directory.Exists(folderWithTestLicenses))
                globalConfig.DataCustomFolder = folderWithTestLicenses;

            // Try to reset some special static variables which may cary over through many tests
            SystemFingerprint.ResetForTest();
        }

    }
}
