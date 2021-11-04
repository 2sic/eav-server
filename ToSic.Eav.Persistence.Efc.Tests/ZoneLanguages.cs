using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;

namespace ToSic.Eav.Persistence.Efc.Tests
{
    [TestClass]
    public class ZoneLanguages : Efc11TestBase
    {
        private const int MinZones = 2;
        private const int MaxZones = 5;
        private const int AppCountInHomeZone = 5;
        private const int ZoneCountWithMultiLanguage = 2;
        
        private const int ZoneHome = 2;
        private const int ZoneHomeLanguages = 2;
        private const int ZoneHomeLangsActive = 1;

        private const int ZoneMl = 3;
        private const int ZoneMlLanguages = 3;
        private const int ZoneMlLangsActive = 2;

        private const int ZonesWithInactiveLanguages = 2;

        [TestInitialize]
        public new void Init()
        {
            // call init to get loader etc. ready
            base.Init();
            _zones = Loader.Zones();
            _defaultAppId = _zones.First().Value.DefaultAppId;
            _appsInHomeZone = _zones[ZoneHome].Apps;
        }


        private IReadOnlyDictionary<int, Zone> _zones;
        private int _defaultAppId;
        private Dictionary<int, string> _appsInHomeZone;

        [TestMethod]
        public void ZonesWithAndWithoutLanguages()
        {
            var mlZones = _zones.Values.Where(z => z.Languages.Count > 1).ToList();
            Assert.AreEqual(ZoneCountWithMultiLanguage, mlZones.Count, $"should have {ZoneCountWithMultiLanguage} ml zones");
            var mlWithInactive = mlZones.Where(z => z.Languages.Any(l => !l.Active)).ToList();
            Assert.AreEqual(ZonesWithInactiveLanguages, mlWithInactive.Count, $"expect {ZonesWithInactiveLanguages} to have inactive languages");
        }

        [TestMethod]
        public void HomeZoneLanguages()
        {
            var firstMl = _zones[ZoneHome];
            Assert.AreEqual(ZoneHomeLanguages, firstMl.Languages.Count, $"the first zone with ML should have {ZoneHomeLanguages} languages");
            Assert.AreEqual(ZoneHomeLangsActive, firstMl.Languages.Count(l => l.Active), $"{ZoneHomeLangsActive} are active");
        }

        [TestMethod]
        public void ZoneMultiLanguages()
        {
            var firstMl = _zones[ZoneMl];
            Assert.AreEqual(ZoneMlLanguages, firstMl.Languages.Count, $"the first zone with ML should have {ZoneHomeLanguages} languages");
            Assert.AreEqual(ZoneMlLangsActive, firstMl.Languages.Count(l => l.Active), $"{ZoneHomeLangsActive} are active");
        }

        [TestMethod]
        public void CountAppsOnMlZone() => Assert.AreEqual(AppCountInHomeZone, _appsInHomeZone.Count, "app count on second zone");

        [TestMethod]
        public void HatAtLeastExpertedZoneCount() =>
            Assert.IsTrue(_zones.Count > MinZones && _zones.Count < MaxZones,
                $"zone count - often changes, as new test-portals are made. Found: {_zones.Count}");

        [TestMethod]
        public void DefaultAppIs1() => Assert.AreEqual(1, _defaultAppId, "def app on first zone");
    }
}
