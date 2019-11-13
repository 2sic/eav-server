using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.App;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Persistence.Efc.Tests
{
    [TestClass]
    public class Efc11LoadTests : Efc11TestBase
    {
        [TestMethod]
        public void GetSomething()
        {
            var results = Db.ToSicEavZones.Single(z => z.ZoneId == 1);
            Assert.IsTrue(results.ZoneId == 1, "zone doesn't fit - it is " + results.ZoneId);
        }

        [Ignore]
        [TestMethod]
        public void TestLoadXAppBlog()
        {
            var results = TestLoadApp(2);

            Assert.IsTrue(results.List.Count() > 1097 && results.List.Count() < 1200, "tried counting entities on the blog-app");
        }

        [Ignore]
        [TestMethod]
        public void PerformanceLoading100XBlog()
        {
            var loadCount = 25;
            for (int i = 0; i < loadCount; i++)
            {
                Loader = NewLoader();
                TestLoadApp(2);
            }
        }


        [TestMethod]
        public void LoadContentTypesOf2Once()
        {
            var results = TestLoadCts(2);
            Assert.AreEqual(61, results.Count, "dummy test: ");
        }

        [TestMethod]
        public void LoadContentTypesOf2TenXCached()
        {
            //Loader.ResetCacheForTesting();
            var results = TestLoadCts(2);
            for (var x = 0; x < 9; x++)
                results = TestLoadCts(2);
            Assert.AreEqual(61, results.Count, "dummy test: ");
        }

        [TestMethod]
        public void LoadContentTypesOf2TenXCleared()
        {
            var results = TestLoadCts(2);
            for (var x = 0; x < 9; x++)
            {
                results = TestLoadCts(2);
                //Loader.ResetCacheForTesting();
            }
            // var str = results.ToString();
            Assert.AreEqual(61, results.Count, "dummy test: ");
        }

        [TestMethod]
        public void TestMetadataTargetTypes()
        {
            var types = Factory.Resolve<ITargetTypes>().TargetTypes;

            Assert.AreEqual(10, types.Count);
            Assert.IsTrue(types[Constants.NotMetadata] == "Default");
        }

        [TestMethod]
        public void TestZonesLoader_WithLanguageDef()
        {
            var zones = Loader.Zones();
            var defapp = zones.First().Value.DefaultAppId;
            var apps = zones[2].Apps;

            Assert.AreEqual(1, defapp, "def app on first zone");
            Assert.IsTrue(zones.Count > 70 && zones.Count < 100, "zone count - often changes, as new test-portals are made");
            Assert.AreEqual(24, apps.Count, "app count on second zone");

            // ML Checks
            var mlZones = zones.Values.Where(z => z.Languages.Count > 1).ToList();
            Assert.AreEqual(9, mlZones.Count, "should have 9 ml zones");
            var firstMl = mlZones.First();
            Assert.AreEqual(2, firstMl.Languages.Count, "think that first zone with ML should have 2 languages");
            Assert.AreEqual(2, firstMl.Languages.Count(l => l.Active = true), "two are active");

            var mlWithInactive = mlZones.Where(z => z.Languages.Any(l => !l.Active)).ToList();
            Assert.AreEqual(0, mlWithInactive.Count, "expect 2 to have inactive languages");


        }

        private AppDataPackage TestLoadApp(int appId) => Loader.AppPackage(appId);

        private IList<IContentType> TestLoadCts(int appId) => Loader.ContentTypes(appId, null);
    }
}
