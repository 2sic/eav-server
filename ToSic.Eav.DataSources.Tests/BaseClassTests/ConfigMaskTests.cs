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



        private TestDataSourceBase GetDs() => this.GetTestDataSource<TestDataSourceBase>();

    }
}
