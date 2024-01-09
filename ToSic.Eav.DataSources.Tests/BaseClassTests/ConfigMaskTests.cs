using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ToSic.Eav.DataSource.Internal;
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
            ds.ConfigMask("Something", $"[{DataSourceConstants.MyConfigurationSourceName}:Test]");
            var ccc = ds.CacheRelevantConfigurations.FirstOrDefault();
            AreEqual("Something", ccc);

            var pair = ds.Configuration.Values.FirstOrDefault();
            AreEqual("Something", pair.Key);
            AreEqual($"[{DataSourceConstants.MyConfigurationSourceName}:Test]", pair.Value);
        }



        private TestDataSourceBase GetDs() => CreateDataSource<TestDataSourceBase>();

    }
}
