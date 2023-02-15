using System.IO;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Configuration;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Eav.StartUp;

namespace ToSic.Testing.Shared
{
    public abstract class TestBaseEav : TestBaseForIoC
    {
        protected TestBaseEav(TestConfiguration testConfiguration = default) : base(testConfiguration)
        {
        }

        protected override void SetupServices(IServiceCollection services)
        {
            base.SetupServices(services);
            if (TestConfiguration.AddAllEavServices)
                services
                    .AddEav();
        }

        /// <summary>
        /// Run configure steps
        /// </summary>
        protected override void Configure()
        {
            base.Configure();

            // this will run after the base constructor, which configures DI
            var dbConfiguration = Build<IDbConfiguration>();
            dbConfiguration.ConnectionString = TestConfiguration.ConStr;

            StartupGlobalFoldersAndFingerprint();
        }


        protected void StartupGlobalFoldersAndFingerprint()
        {
            var globalConfig = Build<IGlobalConfiguration>();
            globalConfig.GlobalFolder = TestConfiguration.GlobalFolder; // "c:\\Projects\\2sxc\\2sxc\\Src\\Data\\";
            if (Directory.Exists(TestConfiguration.GlobalDataCustomFolder)) 
                globalConfig.DataCustomFolder = TestConfiguration.GlobalDataCustomFolder; // "C:\\Projects\\2sxc\\2sxc-dev-materials\\App_Data\\system-custom\\";
            
            // Try to reset some special static variables which may cary over through many tests
            SystemFingerprint.ResetForTest();
        }

    }
}
