using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Testing.Shared;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Eav.DataSourceTests.BaseClassTests
{
    [TestClass]
    public class ConfigMaskTests: TestBaseEavDataSource
    {
        [TestMethod]
        public void ConfigMaskClassic()
        {
            var ds = GetDs();
            ds.ConfigMask("Something", $"[{ToSic.Eav.DataSource.DataSourceBase.MyConfiguration}:Test]");
            var ccc = ds.CacheRelevantConfigurations.FirstOrDefault();
            AreEqual("Something", ccc);

            var pair = ds.Configuration.Values.FirstOrDefault();
            AreEqual("Something", pair.Key);
            AreEqual($"[{ToSic.Eav.DataSource.DataSourceBase.MyConfiguration}:Test]", pair.Value);
        }



        private TestDataSourceBase GetDs() => CreateDataSource<TestDataSourceBase>();

    }
}
