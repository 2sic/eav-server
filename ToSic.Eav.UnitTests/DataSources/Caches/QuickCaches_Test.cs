using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.UnitTests.DataSources.Caches
{
    [TestClass]
    public class QuickCaches_Test
    {

        [TestMethod]
        public void QuickCache_AddListAndCheckIfIn()
        {
            const string ItemToFilter = "1023";
            var ds = CreateFilterForTesting(100, ItemToFilter);

            var cache = ds.Cache;
            var listCache = cache;// as IListCache;
            Assert.IsFalse(listCache.ListHas(ds.CacheFullKey), "Should not have it in cache yet");

            // manually add to cache
            // listCache.ListSet(ds.CacheFullKey, ds.LightList, ds.CacheLastRefresh);
            listCache.ListSet(ds[Constants.DefaultStreamName]);
            Assert.IsTrue(listCache.ListHas(ds.CacheFullKey + "|Default"), "Should have it in cache now");
            Assert.IsTrue(listCache.ListHas(ds[Constants.DefaultStreamName]), "Should also have the DS default");
            
            Assert.IsTrue(listCache.ListHas(ds[Constants.DefaultStreamName]), "should have it by stream as well");
            

            // Try to auto-retrieve 
            var cached = listCache.ListGet(ds.CacheFullKey + "|Default").LightList;

            Assert.AreEqual(1, cached.Count());

            cached = listCache.ListGet(ds[Constants.DefaultStreamName]).LightList;
            Assert.AreEqual(1, cached.Count());

            var lci = listCache.ListGet(ds.CacheFullKey);
            Assert.AreEqual(null, lci, "Cached should be null because the name isn't correct");

            lci = listCache.ListGet(ds[Constants.DefaultStreamName]);
            Assert.AreNotEqual(null, lci, "Cached should be found because usin stream instead of name");

            cached = listCache.ListGet(ds[Constants.DefaultStreamName]).LightList;
            Assert.AreEqual(1, cached.Count());

        }

        [TestMethod]
        public void QuickCache_AddAndWaitForReTimeout()
        {
            const string ItemToFilter = "1027";
            var ds = CreateFilterForTesting(100, ItemToFilter);

            var listCache = ds.Cache; //as IListCache;
            listCache.ListDefaultRetentionTimeInSeconds = 1;
            Assert.IsFalse(listCache.ListHas(ds.CacheFullKey), "Should not have it in cache yet");

            listCache.ListSet(ds.CacheFullKey, ds.LightList, ds.CacheLastRefresh);
            Assert.IsTrue(listCache.ListHas(ds.CacheFullKey), "Should have it in cache now");

            Thread.Sleep(400);
            Assert.IsTrue(listCache.ListHas(ds.CacheFullKey), "Should STILL be in cache");

            Thread.Sleep(601);
            Assert.IsFalse(listCache.ListHas(ds.CacheFullKey), "Should NOT be in cache ANY MORE");
        }




        public static EntityIdFilter CreateFilterForTesting(int testItemsInRootSource, string entityIdsValue)
        {
            var ds = DataTableDataSourceTest.GeneratePersonSourceWithDemoData(testItemsInRootSource, 1001);
            var filtered = new EntityIdFilter();
            filtered.ConfigurationProvider = ds.ConfigurationProvider;
            filtered.Attach(ds);
            filtered.EntityIds = entityIdsValue;
            return filtered;
        }
    }
}
