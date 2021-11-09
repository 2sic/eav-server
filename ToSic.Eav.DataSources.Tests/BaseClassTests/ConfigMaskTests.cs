using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.BaseClassTests
{
    [TestClass]
    public class ConfigMaskTests: TestBaseDiEavFullAndDb
    {
        [TestMethod]
        public void ConfigMaskClassic()
        {
            var ds = GetDs();
            ds.ConfigMask("Something", "[Settings:Test]");
            var ccc = ds.CacheRelevantConfigurations.FirstOrDefault();
            Assert.AreEqual("Something", ccc);

            var pair = ds.Configuration.Values.FirstOrDefault();
            Assert.AreEqual("Something", pair.Key);
            Assert.AreEqual("[Settings:Test]", pair.Value);
        }

        [DataRow("Test", "Test", "[Settings:Test]")]
        [DataRow("FirstName", "FirstName", "[Settings:FirstName]")]
        [DataRow("Birthday|yyyy-MM-dd", "Birthday", "[Settings:Birthday|yyyy-MM-dd]")]
        [DataTestMethod]
        public void ConfigMaskQuick(string keyAndMask, string key, string expected)
        {
            var ds = GetDs();
            ds.ConfigMask(keyAndMask);
            var ccc = ds.CacheRelevantConfigurations.FirstOrDefault();
            Assert.AreEqual(key, ccc, $"Key in CCC should be '{key}' but was '{ccc}'");

            var pair = ds.Configuration.Values.FirstOrDefault();
            Assert.AreEqual(key, pair.Key, $"Key in pair should be '{key}' but is '{pair.Key}'");
            Assert.AreEqual(expected, pair.Value, $"Value in pair should be '{expected}' but is '{pair.Value}'");
        }

        private TestDataSourceBase GetDs()
        {
            return DataSourceFactory.GetDataSource<TestDataSourceBase>(DataSourceFactory.GetPublishing(new AppIdentity(0, 0)));
        }
    }
}
