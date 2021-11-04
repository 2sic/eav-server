using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests
{
    // Todo
    // Create tests with language-parameters as well, as these tests ignore the language and always use default

    [TestClass]
    public class PagingTest: TestBaseDiEavFullAndDb
    {
        private readonly int seedId = 1001;

        [TestMethod]
        public void Paging_BasicPagingPg1On1000Items()
        {
            var ds = CreatePagingForTesting(1000);
            var pgStream = ds["Paging"].ListForTests().First();
            Assert.AreEqual(1.ToDecimal(), pgStream.Value<int>("PageNumber"));
            Assert.AreEqual(10.ToDecimal(), pgStream.Value<int>("PageSize"));
            Assert.AreEqual(100.ToDecimal(), pgStream.Value<int>("PageCount"));
            Assert.AreEqual(1000.ToDecimal(), pgStream.Value<int>("ItemCount"));

            var result = ds.ListForTests();
            Assert.AreEqual(10, result.Count());
            Assert.AreEqual(seedId, result.First().EntityId);
        }

        [TestMethod]
        public void Paging_BasicPagingPg7On1001Items()
        {
            var ds = CreatePagingForTesting(1001);
            ds.PageNumber = 7;
            var pgstream = ds["Paging"].ListForTests().First();
            Assert.AreEqual(7.ToDecimal(), pgstream.Value<int>("PageNumber"));
            Assert.AreEqual(10.ToDecimal(), pgstream.Value<int>("PageSize"));
            Assert.AreEqual(101.ToDecimal(), pgstream.Value<int>("PageCount"));
            Assert.AreEqual(1001.ToDecimal(), pgstream.Value<int>("ItemCount"));

            var result = ds.ListForTests();
            Assert.AreEqual(10, result.Count());
            Assert.AreEqual(seedId + 60, result.First().EntityId);
        }

        [TestMethod]
        public void Paging_BasicPagingPg50On223Items()
        {
            var ds = CreatePagingForTesting(223);
            ds.PageSize = 50;
            ds.PageNumber = 5;
            var pgstream = ds["Paging"].ListForTests().First();
            Assert.AreEqual(5.ToDecimal(), pgstream.Value<int>("PageNumber"));
            Assert.AreEqual(50.ToDecimal(), pgstream.Value<int>("PageSize"));
            Assert.AreEqual(5.ToDecimal(), pgstream.Value<int>("PageCount"));
            Assert.AreEqual(223.ToDecimal(), pgstream.Value<int>("ItemCount"));

            var result = ds.ListForTests();
            Assert.AreEqual(23, result.Count());
            Assert.AreEqual(seedId + 200, result.First().EntityId);
            Assert.AreEqual(seedId + 223 -1, result.Last().EntityId);
        }

        [TestMethod]
        public void Paging_CacheKeySimple()
        {
            var ds = CreatePagingForTesting(45);
            Assert.AreEqual("Paging:NoGuid&PageSize=10&PageNumber=1", ds.CachePartialKey);
            Assert.AreEqual("DataTable:NoGuid&TitleField=FullName&EntityIdField=EntityId&ModifiedField=InternalModified&ContentType=Person" +
                            ">Paging:NoGuid&PageSize=10&PageNumber=1", ds.CacheFullKey);
            var lastRefresh = ds.CacheTimestamp; // get this before comparison, because sometimes slow execution will get strange results
            Assert.IsTrue(DateTime.Now.Ticks >= lastRefresh, "Date-check of cache refresh");
        }

        // to test
        // pickup of settings when it has settings
        
        public DataSources.Paging CreatePagingForTesting(int testItemsInRootSource)
        {
            var ds = new DataTablePerson(this).Generate(testItemsInRootSource, seedId);
            return Build<DataSourceFactory>().GetDataSource<DataSources.Paging>(ds);
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
