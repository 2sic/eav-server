﻿using System.IO;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Configuration;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Eav.StartUp;

namespace ToSic.Testing.Shared
{
    public abstract class TestBaseEav : TestBaseLib
    {
        protected override void SetupServices(IServiceCollection services)
        {
            base.SetupServices(services);
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
            var dbConfiguration = GetService<IDbConfiguration>();
            dbConfiguration.ConnectionString = TestConfiguration.ConStr;

            StartupGlobalFoldersAndFingerprint();
        }


        protected void StartupGlobalFoldersAndFingerprint()
        {
            var globalConfig = GetService<IGlobalConfiguration>();
            globalConfig.GlobalFolder = TestConfiguration.GlobalFolder;
            if (Directory.Exists(TestConfiguration.GlobalDataCustomFolder)) 
                globalConfig.DataCustomFolder = TestConfiguration.GlobalDataCustomFolder;

            // Try to reset some special static variables which may cary over through many tests
            SystemFingerprint.ResetForTest();
        }

    }
}
