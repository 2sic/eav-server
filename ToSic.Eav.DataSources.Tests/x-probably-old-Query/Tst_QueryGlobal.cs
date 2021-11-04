using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.ImportExport.Tests.Persistence.File;
using ToSic.Eav.Logging;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.Query
{
    [TestClass]
    [DeploymentItem("..\\..\\" + TestConfig.GlobalQueriesData, TestConfig.TestingPath)]
    public class Tst_QueryGlobal: TestBaseDiEavFull
    {
        private readonly GlobalQueries _globalQueries;
        private readonly QueryBuilder _queryBuilder;

        public Tst_QueryGlobal()
        {
            _globalQueries = Build<GlobalQueries>();
            _queryBuilder = Build<QueryBuilder>();
        }

        private const int testQueryCount = 7;

        [ClassInitialize]
        public static void Config(TestContext context)
        {
            TestGlobalFolderRepository.PathToUse = TestConfig.TestingPath;
        }

        [TestMethod]
        public void FindGlobalQueries()
        {
            var queries = _globalQueries.AllQueries();
            Assert.AreEqual(testQueryCount, queries.Count, $"should find {testQueryCount} query definitions");
        }


        [TestMethod]
        public void ReviewGlobalZonesQuery()
        {
            var queryName = "Eav.Queries.Global.Zones";
            var queryEnt = _globalQueries.FindQuery(queryName);
            Assert.AreEqual(queryName, queryEnt.Value<string>("Name"), "should find zones");

            var qdef = new QueryDefinition(queryEnt, queryEnt.AppId, null);
            Assert.AreEqual(2, qdef.Parts.Count, "counting parts of the qdef, should have the zone + sort = 2 parts");
        }

        [TestMethod]
        public void UseGlobalZonesQuery()
        {
            var queryEnt = _globalQueries.FindQuery("Eav.Queries.Global.Zones");

            var qDef = new QueryDefinition(queryEnt, TestConfig.AppForQueryTests, null);

            var fac = _queryBuilder.Init(Log); //new QueryBuilder(Log);
            var query = fac.GetDataSourceForTesting(qDef, false).Item1;

            var list = query.ListForTests();
            Assert.IsTrue(list.Count() > 1, "should find a few portals in the eav-testing-DB");
        }


    }
}
