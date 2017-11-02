using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav;
using ToSic.Eav.Core.Tests.Mocks;
using ToSic.Eav.Implementations.Runtime;
using ToSic.Eav.Implementations.UserInformation;
using ToSic.Eav.Implementations.ValueConverter;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence.Efc.Diagnostics;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Testing.Shared
{
    [TestClass]
    public class InitializeTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            ConfigureEfcDi();
            //AddSqlLogger();
            Factory.Debug = true;
        }

        public static void ConfigureEfcDi() => ConfigureEfcDi(sc => { });

        public static void ConfigureEfcDi(Factory.ServiceConfigurator configure)
        {
            Configuration.SetConnectionString(TestConstants.ConStr);
            Factory.ActivateNetCoreDi(sc =>
            {
                sc.AddTransient<IEavValueConverter, MockValueConverter>();
                sc.AddTransient<IEavUserInformation, NeutralEavUserInformation>();
                sc.AddTransient<IEavUserInformation, NeutralEavUserInformation>();
                sc.AddTransient<IRuntime, NeutralRuntime>();

                configure.Invoke(sc);

                new DependencyInjection().ConfigureNetCoreContainer(sc);
            });

        }

        private static void AddSqlLogger()
        {
            // try to add logger
            var db = Factory.Resolve<EavDbContext>();
            var serviceProvider = db.GetInfrastructure();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            loggerFactory.AddProvider(new EfCoreLoggerProvider());
        }

    }
}
