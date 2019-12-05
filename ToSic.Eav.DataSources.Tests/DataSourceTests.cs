using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.DataSources.Tests
{
    [TestClass]
    public class DataSourceTests
    {
        public const int StandardInstalledDSCount = 20;

        public const int StandardInstalledPipeLineDS = 26;
        public const string SqlFullName = "ToSic.Eav.DataSources.SqlDataSource";
        public const string DeferredFullName = "ToSic.Eav.DataSources.DeferredPipelineQuery";

        [Ignore] // disabled for now, as the SqlDs doesn't have a code-version any more
        [TestMethod]
        public void AutoFindAllDataSources()
        {
            var dsList = DataSource.GetInstalledDataSources(false);
            Assert.AreEqual(StandardInstalledDSCount, dsList.Count(), "expect a correct number of DSs");

            var hasSqlDs = dsList.FirstOrDefault(c => c.Type.FullName == SqlFullName);
            Assert.IsNotNull(hasSqlDs, "should find sql-data source");
        }

        [TestMethod]
        public void AutoFindPipelineDataSources()
        {
            var dsList = DataSource.GetInstalledDataSources(true);
            Assert.AreEqual(StandardInstalledPipeLineDS, dsList.Count(), "expect a correct number of DSs");

            var hasSqlDs = dsList.FirstOrDefault(c => c.Type.FullName == SqlFullName);
            Assert.IsNotNull(hasSqlDs, "should NOT find sql-data source");

            var shouldNotFind = dsList.FirstOrDefault(c => c.Type.FullName == DeferredFullName);
            Assert.IsNull(shouldNotFind, "should NOT find sql-data source");
        }

    }
}
