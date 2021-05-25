using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.Metadata;
using ToSic.Testing.Shared;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Persistence.Efc.Tests
{
    [TestClass]
    public class Efc11LoadTests : Efc11TestBase
    {
        private readonly ITargetTypes _targetTypes;
        public  Efc11LoadTests(): base()
        {
            _targetTypes = EavTestBase.Resolve<ITargetTypes>();
        }

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
            const int loadCount = 25;
            for (var i = 0; i < loadCount; i++)
            {
                Loader = NewLoader();
                TestLoadApp(2);
            }
        }

        private const int ExpectedContentTypesOnApp2 = 44;

        [TestMethod]
        public void LoadContentTypesOf2Once()
        {
            var results = TestLoadCts(2);
            Assert.AreEqual(ExpectedContentTypesOnApp2, results.Count, "dummy test: ");
        }

        [TestMethod]
        public void LoadContentTypesOf2TenXCached()
        {
            //Loader.ResetCacheForTesting();
            var results = TestLoadCts(2);
            for (var x = 0; x < 9; x++)
                results = TestLoadCts(2);
            Assert.AreEqual(ExpectedContentTypesOnApp2, results.Count, "dummy test: ");
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
            Assert.AreEqual(ExpectedContentTypesOnApp2, results.Count, "dummy test: ");
        }

        [TestMethod]
        public void TestMetadataTargetTypes()
        {
            var types = _targetTypes.TargetTypes;

            Assert.AreEqual(10, types.Count);
            Assert.IsTrue(types[(int)TargetTypes.None] == "Default");
        }

        private const int MinZones = 2;
        private const int MaxZones = 5;
        private const int AppCountInTestZone = 6;
        private const int ZonesWithML = 2;
        private const int LanguagesInZ1 = 2;
        private const int ActiveLangsInZ1 = 1;
        private const int InactiveLangsInZ1 = 2;

        [TestMethod]
        public void TestZonesLoader_WithLanguageDef()
        {
            var zones = Loader.Zones();
            var defapp = zones.First().Value.DefaultAppId;
            var apps = zones[2].Apps;

            Assert.AreEqual(1, defapp, "def app on first zone");
            Assert.IsTrue(zones.Count > MinZones && zones.Count < MaxZones, $"zone count - often changes, as new test-portals are made. Found: {zones.Count}");
            Assert.AreEqual(AppCountInTestZone, apps.Count, "app count on second zone");

            // ML Checks
            var mlZones = zones.Values.Where(z => z.Languages.Count > 1).ToList();
            Assert.AreEqual(ZonesWithML, mlZones.Count, $"should have {ZonesWithML} ml zones");
            var firstMl = mlZones.First();
            Assert.AreEqual(LanguagesInZ1, firstMl.Languages.Count, $"the first zone with ML should have {LanguagesInZ1} languages");
            Assert.AreEqual(ActiveLangsInZ1, firstMl.Languages.Count(l => l.Active), $"{ActiveLangsInZ1} are active");

            var mlWithInactive = mlZones.Where(z => z.Languages.Any(l => !l.Active)).ToList();
            Assert.AreEqual(InactiveLangsInZ1, mlWithInactive.Count, $"expect {InactiveLangsInZ1} to have inactive languages");


        }

        private AppState TestLoadApp(int appId) => Loader.AppState(appId, false);

        private IList<IContentType> TestLoadCts(int appId) => Loader.ContentTypes(appId, null);
    }
}
