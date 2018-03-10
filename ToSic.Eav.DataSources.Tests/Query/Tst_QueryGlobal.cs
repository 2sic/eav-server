using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.ImportExport.Tests.Persistence.File;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;

namespace ToSic.Eav.DataSources.Tests.Query
{
    [TestClass]
    [DeploymentItem("..\\..\\" + TestConfig.GlobalQueriesData, TestConfig.TestingPath)]
    public class Tst_QueryGlobal: HasLog
    {
        private const int testQueryCount = 2;

        public Tst_QueryGlobal() : base("Tst.QryGlb") { }

        [ClassInitialize]
        public static void Config(TestContext context)
        {
            RepositoryInfoOfTestSystem.PathToUse = TestConfig.TestingPath;
        }

        [TestMethod]
        public void FindGlobalQueries()
        {
            var queries = Global.AllQueries();
            Assert.AreEqual(testQueryCount, queries.Count, "should find 2 query definitions");
        }


        [TestMethod]
        public void ReviewGlobalZonesQuery()
        {
            var queryName = "Eav.Queries.Global.Zones";
            var queryEnt = Global.FindQuery(queryName);
            Assert.AreEqual(queryName, queryEnt.GetBestValue("Name").ToString(), "should find zones");

            var qdef = new QueryDefinition(queryEnt);
            Assert.AreEqual(2, qdef.Parts.Count, "counting parts of the qdef, should have the zone + sort = 2 parts");
        }

        [TestMethod]
        public void UseGlobalZonesQuery()
        {
            var queryEnt = Global.FindQuery("Eav.Queries.Global.Zones");

            var qdef = new QueryDefinition(queryEnt, TestConfig.AppForQueryTests);

            var fac = new Pipeline.DataPipelineFactory(Log);
            var query = fac.GetDataSourceForTesting(qdef, false);

            var list = query.List;
            Assert.IsTrue(list.Count() > 1, "should find a few portals");
        }


    }
}
