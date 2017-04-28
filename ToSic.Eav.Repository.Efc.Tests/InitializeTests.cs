using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Implementations.UserInformation;
using ToSic.Eav.Implementations.ValueConverter;
using ToSic.Eav.ImportExport.Interfaces;
using ToSic.Eav.Persistence.Efc.Diagnostics;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repository.Efc.Tests.Mocks;

namespace ToSic.Eav.Repository.Efc.Tests
{
    [TestClass]
    class InitializeTests
    {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            ConfigureDependencyInjection();
        }

        private static void ConfigureDependencyInjection()
        {
            var conStr = @"Data Source=(local)\SQLEXPRESS;Initial Catalog=""2flex 2Sexy Content"";Integrated Security=True;";
            Configuration.SetConnectionString(conStr);
            var cont = Factory.Container;

            cont.RegisterType<IImportExportEnvironment, ImportExportEnvironmentMock>();
            cont.RegisterType(typeof(IEavValueConverter), typeof(NeutralValueConverter), new InjectionConstructor());
            cont.RegisterType(typeof(IEavUserInformation), typeof(NeutralEavUserInformation), new InjectionConstructor());

            new DependencyInjection().ConfigureDefaultMappings(cont);

            // try to add logger
            var db = Factory.Resolve<EavDbContext>();
            var serviceProvider = db.GetInfrastructure();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            loggerFactory.AddProvider(new EfCoreLoggerProvider());
        }
    }
}
