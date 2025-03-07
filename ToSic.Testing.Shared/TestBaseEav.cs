using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Integration;
using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Security.Fingerprint;

namespace ToSic.Testing.Shared;

public abstract class TestBaseEav(EavTestConfig testConfig = default) : TestBaseEavDataSource(testConfig)
{
    protected override IServiceCollection SetupServices(IServiceCollection services)
    {
        //base.SetupServices(services);

        // Just add all services, not perfect yet
        // Ideally should only add the services not added by previous layers
        return services
            .AddEavEverything();
    }

    /// <summary>
    /// Run configure steps
    /// </summary>
    protected override void Configure()
    {
        // The base has an empty configure, and we want to clearly not call it
        //base.Configure();

        // this will run after the base constructor, which configures DI
        //var dbConfiguration = GetService<IDbConfiguration>();
        //dbConfiguration.ConnectionString = TestConfig.ConStr;

        var fixture = new FullDbFixtureHelper(GetService<IServiceProvider>());
        fixture.Configure(TestConfig);
        //fixture.StartupGlobalFoldersAndFingerprint();

        //StartupGlobalFoldersAndFingerprint();
    }


    //protected void StartupGlobalFoldersAndFingerprint()
    //{
    //    var globalConfig = GetService<IGlobalConfiguration>();
    //    globalConfig.GlobalFolder = TestConfig.GlobalFolder;
    //    var folderWithTestLicenses = TestConfig.GlobalDataCustomFolder;
    //    if (Directory.Exists(folderWithTestLicenses)) 
    //        globalConfig.DataCustomFolder = folderWithTestLicenses;

    //    // Try to reset some special static variables which may cary over through many tests
    //    SystemFingerprint.ResetForTest();
    //}

}