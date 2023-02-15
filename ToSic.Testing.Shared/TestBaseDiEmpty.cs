using System.IO;
using ToSic.Eav.Configuration;
using ToSic.Eav.Security.Fingerprint;

namespace ToSic.Testing.Shared
{
    public abstract class TestBaseDiEmpty : TestBaseDiRaw
    {
        protected TestBaseDiEmpty(string logName = null) : base(logName)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            Configure();
        }

        protected void StartupGlobalFoldersAndFingerprint()
        {
            var globalConfig = Build<IGlobalConfiguration>();
            globalConfig.GlobalFolder = TestConstants.GlobalFolder; // "c:\\Projects\\2sxc\\2sxc\\Src\\Data\\";
            if (Directory.Exists(TestConstants.GlobalDataCustomFolder)) 
                globalConfig.DataCustomFolder = TestConstants.GlobalDataCustomFolder; // "C:\\Projects\\2sxc\\2sxc-dev-materials\\App_Data\\system-custom\\";
            
            // Try to reset some special static variables which may cary over through many tests
            SystemFingerprint.ResetForTest();
        }

        /// <summary>
        /// Run configure steps
        /// </summary>
        protected virtual void Configure()
        {
            // this will run after the base constructor, which configures DI
            var dbConfiguration = Build<IDbConfiguration>();
            dbConfiguration.ConnectionString = TestConstants.ConStr;

            StartupGlobalFoldersAndFingerprint();
        }

    }
}
