using System;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.UnitTests.DataSources;

namespace ToSic.Eav.UnitTests
{
    // Todo
    // Create tests with language-parameters as well, as these tests ignore the language and always use default

    [TestClass]
    public class Paging_Test
    {
        private int seedId = 1001;

        [TestMethod]
        public void Paging_BasicPagingPg1On1000Items()
        {
            var ds = CreatePagingForTesting(1000);
            var pgstream = ds["Paging"].List[0];
            Assert.AreEqual(1.ToDecimal(), pgstream.GetBestValue("PageNumber"));
            Assert.AreEqual(10.ToDecimal(), pgstream.GetBestValue("PageSize"));
            Assert.AreEqual(100.ToDecimal(), pgstream.GetBestValue("PageCount"));
            Assert.AreEqual(1000.ToDecimal(), pgstream.GetBestValue("ItemCount"));

            var result = ds.List;
            Assert.AreEqual(10, result.Count);
            Assert.AreEqual(seedId, result.First().Value.EntityId);
        }

        [TestMethod]
        public void Paging_BasicPagingPg7On1001Items()
        {
            var ds = CreatePagingForTesting(1001);
            ds.PageNumber = 7;
            var pgstream = ds["Paging"].List[0];
            Assert.AreEqual(7.ToDecimal(), pgstream.GetBestValue("PageNumber"));
            Assert.AreEqual(10.ToDecimal(), pgstream.GetBestValue("PageSize"));
            Assert.AreEqual(101.ToDecimal(), pgstream.GetBestValue("PageCount"));
            Assert.AreEqual(1001.ToDecimal(), pgstream.GetBestValue("ItemCount"));

            var result = ds.List;
            Assert.AreEqual(10, result.Count);
            Assert.AreEqual(seedId + 60, result.First().Value.EntityId);
        }

        [TestMethod]
        public void Paging_BasicPagingPg50On223Items()
        {
            var ds = CreatePagingForTesting(223);
            ds.PageSize = 50;
            ds.PageNumber = 5;
            var pgstream = ds["Paging"].List[0];
            Assert.AreEqual(5.ToDecimal(), pgstream.GetBestValue("PageNumber"));
            Assert.AreEqual(50.ToDecimal(), pgstream.GetBestValue("PageSize"));
            Assert.AreEqual(5.ToDecimal(), pgstream.GetBestValue("PageCount"));
            Assert.AreEqual(223.ToDecimal(), pgstream.GetBestValue("ItemCount"));

            var result = ds.List;
            Assert.AreEqual(23, result.Count);
            Assert.AreEqual(seedId + 200, result.First().Value.EntityId);
            Assert.AreEqual(seedId + 223 -1, result.Last().Value.EntityId);
        }

        [TestMethod]
        public void Paging_CacheKeySimple()
        {
            var ds = CreatePagingForTesting(45);
            Assert.AreEqual("Paging-NoGuid&PageSize=10&PageNumber=1", ds.CachePartialKey);
            Assert.AreEqual("DataTableDataSource-NoGuid&ContentType=Person>Paging-NoGuid&PageSize=10&PageNumber=1", ds.CacheFullKey);
            var lastRefresh = ds.CacheLastRefresh; // get this before comparison, because sometimes slow execution will get strange results
            Assert.IsTrue(DateTime.Now >= lastRefresh, "Date-check of cache refresh");
        }

        // to test
        // pickup of settings when it has settings
        
        public Paging CreatePagingForTesting(int testItemsInRootSource)
        {
            var ds = DataTableDataSourceTest.GeneratePersonSourceWithDemoData(testItemsInRootSource, seedId);
            return DataSource.GetDataSource<Paging>(1, 1, ds);
            //return filtered;
        }

    }

    public static class DecimalHelpers
    {
        public static decimal ToDecimal(this Int32 original)
        {
            return Convert.ToDecimal(original);
        }

    }
}
