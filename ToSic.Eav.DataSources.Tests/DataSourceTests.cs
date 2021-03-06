﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.DataSourceTests
{
    [TestClass]
    public class DataSourceTests
    {
        public const int EavInstalledDsCount = 41;
        public const int TestingAddedDsCount = 1;
        public const int StandardInstalledDSCount = EavInstalledDsCount + TestingAddedDsCount;

        public const int StandardInstalledPipeLineDS = 29;
        public const string SqlFullName = "ToSic.Eav.DataSources.Sql";
        public const string DeferredFullName = "ToSic.Eav.DataSources.DeferredPipelineQuery";

        //[Ignore] // disabled for now, as the SqlDs doesn't have a code-version any more
        [TestMethod]
        public void AutoFindAllDataSources()
        {
            var dsList = Catalog.GetAll(false);
            Assert.AreEqual(StandardInstalledDSCount, dsList.Count(), "expect a correct number of DSs");

            var hasSqlDs = dsList.FirstOrDefault(c => c.Type.FullName == SqlFullName);
            Assert.IsNotNull(hasSqlDs, "should find sql-data source");
        }

        [TestMethod]
        public void AutoFindPipelineDataSources()
        {
            var dsList = Catalog.GetAll(true);
            Assert.AreEqual(StandardInstalledPipeLineDS, dsList.Count(), "expect a correct number of DSs");

            var hasSqlDs = dsList.FirstOrDefault(c => c.Type.FullName == SqlFullName);
            Assert.IsNotNull(hasSqlDs, "should find sql-data source");

            var shouldNotFind = dsList.FirstOrDefault(c => c.Type.FullName == DeferredFullName);
            Assert.IsNull(shouldNotFind, "should NOT find deferred-data source");
        }

    }
}
