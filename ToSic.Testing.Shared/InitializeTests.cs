using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav;
using ToSic.Eav.Implementations.Runtime;
using ToSic.Eav.Implementations.UserInformation;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence.Efc.Diagnostics;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repository.Efc.Implementations;

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

        public static void ConfigureEfcDi(string optionalConnection = null) => ConfigureEfcDi(sc => { }, optionalConnection);

        public static void ConfigureEfcDi(Factory.ServiceConfigurator configure, string optionalConnection = null)
        {
            var con = optionalConnection ?? TestConstants.ConStr;
            Configuration.SetConnectionString(con);

            Factory.ActivateNetCoreDi(sc =>
            {
                //sc.TryAddTransient<IEavUserInformation, NeutralEavUserInformation>();
                sc.TryAddTransient<IUser, NeutralEavUser>();
                sc.TryAddTransient<IRuntime, NeutralRuntime>();

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
