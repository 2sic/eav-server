using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Testing.Shared;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Eav.DataSourceTests.BaseClassTests
{
    [TestClass]
    public class ConfigMaskTests: TestBaseDiEavFullAndDb
    {
        [TestMethod]
        public void ConfigMaskClassic()
        {
            var ds = GetDs();
            ds.ConfigMask("Something", $"[{DataSource.MyConfiguration}:Test]");
            var ccc = ds.CacheRelevantConfigurations.FirstOrDefault();
            AreEqual("Something", ccc);

            var pair = ds.Configuration.Values.FirstOrDefault();
            AreEqual("Something", pair.Key);
            AreEqual($"[{DataSource.MyConfiguration}:Test]", pair.Value);
        }

        [DataRow("Test", "Test", "Test")]
        [DataRow("FirstName", "FirstName", "FirstName")]
        [DataRow("Birthday|yyyy-MM-dd", "Birthday", "Birthday|yyyy-MM-dd")]
        [DataTestMethod]
        public void ConfigMaskQuick(string keyAndMask, string key, string expected)
        {
            var ds = GetDs();

            // change expected to match proper convention
            expected = "[" + DataSource.MyConfiguration + ":" + expected + "]";

            ds.ConfigMask(keyAndMask);
            var ccc = ds.CacheRelevantConfigurations.FirstOrDefault();
            AreEqual(key, ccc, $"Key in CCC should be '{key}' but was '{ccc}'");

            var pair = ds.Configuration.Values.FirstOrDefault();
            AreEqual(key, pair.Key, $"Key in pair should be '{key}' but is '{pair.Key}'");
            AreEqual(expected, pair.Value, $"Value in pair should be '{expected}' but is '{pair.Value}'");
        }


        private TestDataSourceBase GetDs() => this.GetTestDataSource<TestDataSourceBase>();

    }
}
