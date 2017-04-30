using Microsoft.EntityFrameworkCore;
//using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.UnitTests
{
    [TestClass]
    class InitializeTests
    {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            // new DependencyInjection().ConfigureDefaultMappings(Eav.Factory.Container);
            ConfigureDependencyInjection();
        }

        private static void ConfigureDependencyInjection()
        {
            var conStr = @"Data Source=(local)\SQLEXPRESS;Initial Catalog=""2flex 2Sexy Content"";Integrated Security=True;";
            Configuration.SetConnectionString(conStr);
            Eav.Factory.ActivateNetCoreDi(sc =>
            {

            });

            //var container = Eav.Factory.CreateContainer();//.Container;
            //container.RegisterType<EavDbContext>(new HierarchicalLifetimeManager(), new InjectionFactory(
            //    obj =>
            //    {
            //        var opts = new DbContextOptionsBuilder<EavDbContext>();
            //        opts.UseSqlServer(conStr);
            //        return new EavDbContext(opts.Options);
            //    }));
        }
    }
}
