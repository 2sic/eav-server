using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Persistence.EFC11.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Practices.Unity;
using ToSic.Eav.Data;

namespace ToSic.Eav.Persistence.EFC11.Tests
{
    [TestClass]
    public class Efc11LoadTests
    {
        private void ConfigureDependencyInjection()
        {
            var container = Eav.Factory.Container;
            var conStr = @"Data Source=(local)\SQLEXPRESS;Initial Catalog=""2flex 2Sexy Content"";Integrated Security=True;";
            container.RegisterType<EavDbContext>(new HierarchicalLifetimeManager(), new InjectionFactory(
                obj =>
                {
                    var opts = new DbContextOptionsBuilder<EavDbContext>();
                    opts.UseSqlServer(conStr);
                    return new EavDbContext(opts.Options);
                }));
        }
        [TestMethod]
        public void GetSomething()
        {
            ConfigureDependencyInjection();
            using (var db = Factory.Container.Resolve<EavDbContext>()) {
                var results = db.ToSicEavZones.Single(z => z.ZoneId == 1);

                Assert.IsTrue(results.ZoneId == 1, "zone doesn't fit - it is " + results.ZoneId);
            }
        }

        [TestMethod]
        public void TestLoadAppBlog()
        {
            var results = TestLoadApp(2);
            Assert.IsTrue(results.List.Count() == 1000, "tried counting");
        }

        [TestMethod]
        public void TestLoadCTs()
        {
            var results = TestLoadCts(2);
            Assert.AreEqual(results.Count, 1000, "count is off");
            
        }

        private AppDataPackage TestLoadApp(int appId)
        {
            ConfigureDependencyInjection();
            using (var db = Factory.Container.Resolve<EavDbContext>())
            {
                return new LoadEfc11(db).GetAppDataPackage(null, appId, null, false);
            }
        }

        private IDictionary<int, IContentType> TestLoadCts(int appId)
        {
            ConfigureDependencyInjection();
            using (var db = Factory.Container.Resolve<EavDbContext>())
            {
                return new LoadEfc11(db).GetEavContentTypes(appId);
            }
        }
    }
}
