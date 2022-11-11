using System;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Configuration;
using ToSic.Lib.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Security.Fingerprint;

namespace ToSic.Testing.Shared
{
    public abstract class TestBaseDiEmpty : TestBaseDiRaw // HasLog, IServiceBuilder
    {
        //private IServiceProvider ServiceProvider { get; }

        //public T Build<T>() => ServiceProvider.Build<T>();

        protected TestBaseDiEmpty() : this(null) {}

        protected TestBaseDiEmpty(string logName) : base("Tst." + (logName ?? "BaseDI"))
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            //ServiceProvider = SetupServices(new ServiceCollection()).BuildServiceProvider();
            // ReSharper disable once VirtualMemberCallInConstructor
            Configure();
        }

        //protected virtual IServiceCollection SetupServices(IServiceCollection services)
        //{
        //    AddServices(services);
        //    return services;
        //}

        //protected virtual void AddServices(IServiceCollection services) { }

        protected void StartupGlobalFoldersAndFingerprint()
        {
            var globalConfig = Build<IGlobalConfiguration>();
            globalConfig.DataFolder = TestConstants.GlobalDataFolder; // "c:\\Projects\\2sxc\\2sxc\\Src\\Data\\";
            globalConfig.GlobalFolder = globalConfig.DataFolder;
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
