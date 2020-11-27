using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav;
using ToSic.Eav.Context;
using ToSic.Eav.Repository.Efc.Implementations;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Basic;

namespace ToSic.Testing.Shared
{
    [TestClass]
    public class StartupTestingShared
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
                sc.TryAddTransient<IUser, BasicUser>();
                sc.TryAddTransient<IRuntime, BasicRuntime>();
                configure.Invoke(sc);

                sc.AddEav();
            });

        }

        // this helps debug in advanced scenarios
        // hasn't been used since ca. 2017, but keep in case we ever do deep work on the DB again
        // ReSharper disable once UnusedMember.Global
        //private static void AddSqlLogger()
        //{
        //    // try to add logger
        //    var db = Factory.Resolve<EavDbContext>();
        //    var serviceProvider = db.GetInfrastructure();
        //    var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        //    loggerFactory.AddProvider(new EfCoreLoggerProvider());
        //}

    }
}
