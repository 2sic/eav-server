using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.DataSources.Tests
{
    [TestClass]
    public class DataSourcTests
    {
        public const int StandardInstalledDSCount = 22;
        public const int StandardInstalledPipeLineDS = 17;
        public const string SqlFullName = "ToSic.Eav.DataSources.SqlDataSource";
        public const string DeferredFullName = "ToSic.Eav.DataSources.DeferredPipelineQuery";

        [TestMethod]
        public void AutoFindAllDataSources()
        {
            var dsList = DataSource.GetInstalledDataSources(false);
            Assert.AreEqual(StandardInstalledDSCount, dsList.Count(), "expect a correct number of DSs");

            var hasSqlDs = dsList.FirstOrDefault(c => c.FullName == SqlFullName);
            Assert.IsNotNull(hasSqlDs, "should find sql-data source");
        }

        [TestMethod]
        public void AutoFindPipelineDataSources()
        {
            var dsList = DataSource.GetInstalledDataSources(true);
            Assert.AreEqual(StandardInstalledPipeLineDS, dsList.Count(), "expect a correct number of DSs");

            var hasSqlDs = dsList.FirstOrDefault(c => c.FullName == SqlFullName);
            Assert.IsNotNull(hasSqlDs, "should NOT find sql-data source");

            var shouldNotFind = dsList.FirstOrDefault(c => c.FullName == DeferredFullName);
            Assert.IsNull(shouldNotFind, "should NOT find sql-data source");
        }

    }
}
