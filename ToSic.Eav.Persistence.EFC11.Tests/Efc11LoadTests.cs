using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
//using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Persistence.Efc.Tests
{
    [TestClass]
    public class Efc11LoadTests
    {
        #region test preparations

        private EavDbContext _db;
        private Efc11Loader _loader;

        [TestInitialize]
        public void Init()
        {
            Trace.Write("initializing DB & loader");
            _db = Factory.Resolve<EavDbContext>();
            _loader = NewLoader();
        }

        private Efc11Loader NewLoader() => new Efc11Loader(_db);

        #endregion

        [TestMethod]
        public void GetSomething()
        {
            var results = _db.ToSicEavZones.Single(z => z.ZoneId == 1);
            Assert.IsTrue(results.ZoneId == 1, "zone doesn't fit - it is " + results.ZoneId);
        }

        [TestMethod]
        public void TestLoadXAppBlog()
        {
            var results = TestLoadApp(2);

            Assert.IsTrue(results.Entities.Count > 1097 && results.Entities.Count < 1200, "tried counting entities on the blog-app");
        }

        [TestMethod]
        public void PerformanceLoading100xBlog()
        {
            var loadCount = 25;
            for (int i = 0; i < loadCount; i++)
            {
                _loader = NewLoader();
                TestLoadApp(2);
            }


            // Assert.IsTrue(results.Entities.Count > 1097 && results.Entities.Count < 1200, "tried counting entities on the blog-app");
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
            _loader.ResetCacheForTesting();
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
                _loader.ResetCacheForTesting();
            }
            // var str = results.ToString();
            Assert.AreEqual(61, results.Count, "dummy test: ");
        }

        [TestMethod]
        public void TestMetadataTargetTypes()
        {
            var types = _loader.MetadataTargetTypes();

            Assert.AreEqual(100, types.Count);
            Assert.IsTrue(types[Constants.NotMetadata] == "Default");
        }

        [TestMethod]
        public void TestZonesLoader_WithLanguageDef()
        {
            var zones = _loader.Zones();
            var defapp = zones.First().Value.DefaultAppId;
            var apps = zones[2].Apps;

            Assert.AreEqual(1, defapp, "def app on first zone");
            Assert.AreEqual(69, zones.Count, "zone count - often changes, as new test-portals are made");
            Assert.AreEqual(24, apps.Count, "app count on second zone");

            // ML Checks
            var mlZones = zones.Values.Where(z => z.Languages.Count > 1).ToList();
            Assert.AreEqual(8, mlZones.Count, "should have 7 ml zones");
            var firstMl = mlZones.First();
            Assert.AreEqual(2, firstMl.Languages.Count, "think that first zone with ML should have 2 languages");
            Assert.AreEqual(2, firstMl.Languages.Count(l => l.Active = true), "two are active");

            var mlWithInactive = mlZones.Where(z => z.Languages.Any(l => !l.Active)).ToList();
            Assert.AreEqual(0, mlWithInactive.Count, "expect 2 to have inactive languages");


        }

        private AppDataPackage TestLoadApp(int appId)
        {
            return _loader.AppPackage(appId);
        }

        private IDictionary<int, ToSic.Eav.Interfaces.IContentType> TestLoadCts(int appId)
        {
            return _loader.ContentTypes(appId);
        }

    }
}
