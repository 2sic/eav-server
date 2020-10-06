using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Configuration;
using ToSic.Eav.DataSourceTests.ExternalData;

namespace ToSic.Eav.DataSourceTests.EntityFilters
{
    [TestClass]
    public class EntityIdFilter_Test
    {
        [TestMethod]
        public void EntityIdFilter_SingleItem()
        {
            const string ItemToFilter = "1023";
            var filtered = CreateFilterForTesting(100, ItemToFilter);

            // diclist
            //var dicList = filtered.LightList;
            var ll = filtered.Immutable;

            //Assert.AreEqual(ItemToFilter, dicList.First().Value.EntityId.ToString());
            Assert.AreEqual(ItemToFilter, ll.First().EntityId.ToString());
        }


        [TestMethod]
        public void EntityIdFilter_NoItems()
        {
            const string ItemToFilter = "";
            var filtered = CreateFilterForTesting(100, ItemToFilter);
            //var dicList = filtered.List;
            var ll = filtered.Immutable; //.ToList();

            //Assert.AreEqual(0, dicList.Count, "Should return 0 items");
            Assert.AreEqual(0, ll.Count, "Should return 0 items");
        }

        [TestMethod]
        public void EntityIdFilter_MultipleItems()
        {
            const string ItemToFilter = "1011,1023,1050,1003";
            var filtered = CreateFilterForTesting(100, ItemToFilter);

            //var dl = filtered.List;
            //Assert.AreEqual("1011", dl.First().Value.EntityId.ToString(), "Test Dic that sorting IS affeted");
            //Assert.AreEqual(4, dl.Count, "Count after filtering");


            var ll = filtered.Immutable;//.ToList();
            Assert.AreEqual("1011", ll.First().EntityId.ToString(), "Test Light that sorting IS affeted");
            Assert.AreEqual(4, ll.Count, "Count after filtering");
        }

        [TestMethod]
        public void EntityIdFilter_FilterWithSpaces()
        {
            const string ItemToFilter = "1011, 1023 ,1050   ,1003";
            var filtered = CreateFilterForTesting(100, ItemToFilter);

            //var dl = filtered.List;
            //Assert.AreEqual("1011", dl.First().Value.EntityId.ToString(), "Test Dic that sorting IS affeted");
            //Assert.AreEqual(4, dl.Count, "Count after filtering");

            var ll = filtered.Immutable;//.ToList();
            Assert.AreEqual("1011", ll.First().EntityId.ToString(), "Test Light that sorting IS affeted");
            Assert.AreEqual(4, ll.Count, "Count after filtering");
        }

        [TestMethod]
        public void EntityIdFilter_CacheKey1()
        {
            // Simple scenario, 1 filter, no special spaces etc.
            const string ItemToFilter = "1023";
            var ds = CreateFilterForTesting(100, ItemToFilter);

            Assert.AreEqual("EntityIdFilter-NoGuid&EntityIds=1023", ds.CachePartialKey);
            Assert.AreEqual("DataTable-NoGuid&TitleField=FullName&EntityIdField=EntityId&ModifiedField=InternalModified&ContentType=Person" +
                            ">EntityIdFilter-NoGuid&EntityIds=1023", ds.CacheFullKey);
            var lastRefresh = ds.CacheTimestamp; // get this before comparison, because sometimes slow execution will get strange results
            Assert.IsTrue(DateTime.Now.Ticks >= lastRefresh, "Date-check of cache refresh");

        }
        [TestMethod]
        public void EntityIdFilter_CacheKeyMulti()
        {
            // Multi-value scenario, no special spaces etc.
            var ItemToFilter = "1011,1023,1050,1003";
            var ds = CreateFilterForTesting(100, ItemToFilter);

            Assert.AreEqual("EntityIdFilter-NoGuid&EntityIds=1011,1023,1050,1003", ds.CachePartialKey);
            Assert.AreEqual("DataTable-NoGuid&TitleField=FullName&EntityIdField=EntityId&ModifiedField=InternalModified&ContentType=Person" +
                            ">EntityIdFilter-NoGuid&EntityIds=1011,1023,1050,1003", ds.CacheFullKey);

            // Multi-value scenario, special spaces and trailing comma etc.
            ItemToFilter = "1011, 1023  ,   1050,    1003,";
            var partialKey = "EntityIdFilter-NoGuid&EntityIds=1011,1023,1050,1003,";
            ds = CreateFilterForTesting(100, ItemToFilter);
            Assert.AreEqual(partialKey, ds.CachePartialKey);
            Assert.AreEqual("DataTable-NoGuid&TitleField=FullName&EntityIdField=EntityId&ModifiedField=InternalModified&ContentType=Person" +
                            ">" + partialKey, ds.CacheFullKey);

        }

        public static EntityIdFilter CreateFilterForTesting(int testItemsInRootSource, string entityIdsValue)
        {
            var ds = DataTableTst.GeneratePersonSourceWithDemoData(testItemsInRootSource, 1001);
            var filtered = new EntityIdFilter()
                .Init(ds.Configuration.LookUps);
            //filtered.ConfigurationProvider = ds.ConfigurationProvider;
            filtered.Attach(ds);
            filtered.EntityIds = entityIdsValue;
            return filtered;
        }
    }
}
