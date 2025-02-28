using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ToSic.Eav.DataSource;
using ToSic.Testing.Shared;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Eav.DataSourceTests.BaseClassTests;

[TestClass]
public class ConfigMaskTests: TestBaseEavDataSource
{
    private DataSourcesTstBuilder DsSvc => field ??= GetService<DataSourcesTstBuilder>();

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



    private TestDataSourceBase GetDs() => DsSvc.CreateDataSource<TestDataSourceBase>();

}