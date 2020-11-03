using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.ImportExport.Tests.Persistence.File;
using ToSic.Eav.Logging;

namespace ToSic.Eav.DataSourceTests.Query
{
    [TestClass]
    [DeploymentItem("..\\..\\" + TestConfig.GlobalQueriesData, TestConfig.TestingPath)]
    public class Tst_QueryGlobal: HasLog
    {
        private const int testQueryCount = 8;

        public Tst_QueryGlobal() : base("Tst.QryGlb") { }

        [ClassInitialize]
        public static void Config(TestContext context)
        {
            RepositoryInfoOfTestSystem.PathToUse = TestConfig.TestingPath;
        }

        [TestMethod]
        public void FindGlobalQueries()
        {
            var queries = GlobalQueries.AllQueries();
            Assert.AreEqual(testQueryCount, queries.Count, $"should find {testQueryCount} query definitions");
        }


        [TestMethod]
        public void ReviewGlobalZonesQuery()
        {
            var queryName = "Eav.Queries.Global.Zones";
            var queryEnt = GlobalQueries.FindQuery(queryName);
            Assert.AreEqual(queryName, queryEnt.GetBestValue("Name").ToString(), "should find zones");

            var qdef = new QueryDefinition(queryEnt, queryEnt.AppId, null);
            Assert.AreEqual(2, qdef.Parts.Count, "counting parts of the qdef, should have the zone + sort = 2 parts");
        }

        [TestMethod]
        public void UseGlobalZonesQuery()
        {
            var queryEnt = GlobalQueries.FindQuery("Eav.Queries.Global.Zones");

            var qDef = new QueryDefinition(queryEnt, TestConfig.AppForQueryTests, null);

            var fac = Factory.Resolve<QueryBuilder>().Init(Log); //new QueryBuilder(Log);
            var query = fac.GetDataSourceForTesting(qDef, false);

            var list = query.List;
            Assert.IsTrue(list.Count() > 1, "should find a few portals in the eav-testing-DB");
        }


    }
}
