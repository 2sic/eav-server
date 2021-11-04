using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav;
using ToSic.Eav.Configuration;
using ToSic.Eav.Context;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;

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

        public static void ConfigureEfcDi(Factory.ServiceConfigurator configure = null, string optionalConnection = null)
        {
            Factory.ActivateNetCoreDi(sc =>
            {
                configure?.Invoke(sc);
                sc.AddEav();
            });
            var con = optionalConnection ?? TestConstants.ConStr;
            EavTestBase.Resolve<IDbConfiguration>().ConnectionString = con;
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
