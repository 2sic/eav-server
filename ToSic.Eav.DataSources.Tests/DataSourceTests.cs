using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources.Catalog;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests
{
    [TestClass]
    public class DataSourceTests: TestBaseEavDataSource
    {
        public const int EavInstalledDsCount = 40; // with 2sxc it's ca. 48;
        public const int TestingAddedDsCount = 1;
        public const int StandardInstalledDSCount = EavInstalledDsCount + TestingAddedDsCount;

        public const int StandardInstalledPipeLineDS = 33; // with 2sxc it's ca. 39;
        public const string SqlFullName = "ToSic.Eav.DataSources.Sql";
        public const string DeferredFullName = "ToSic.Eav.DataSources.DeferredPipelineQuery";

        //[Ignore] // disabled for now, as the SqlDs doesn't have a code-version any more
        [TestMethod]
        public void AutoFindAllDataSources()
        {
            var dsCatalog = GetService<DataSourceCatalog>();
            var dsList = DataSourceCatalog.GetAll(false);
            Assert.AreEqual(StandardInstalledDSCount, dsList.Count(), "expect a correct number of DSs");

            var hasSqlDs = dsList.FirstOrDefault(c => c.Type.FullName == SqlFullName);
            Assert.IsNotNull(hasSqlDs, "should find sql-data source");
        }

        [TestMethod]
        public void AutoFindPipelineDataSources()
        {
            var dsList = DataSourceCatalog.GetAll(true);
            Assert.AreEqual(StandardInstalledPipeLineDS, dsList.Count(), "expect a correct number of DSs");

            var hasSqlDs = dsList.FirstOrDefault(c => c.Type.FullName == SqlFullName);
            Assert.IsNotNull(hasSqlDs, "should find sql-data source");

            var shouldNotFind = dsList.FirstOrDefault(c => c.Type.FullName == DeferredFullName);
            Assert.IsNull(shouldNotFind, "should NOT find deferred-data source");
        }

    }
}
