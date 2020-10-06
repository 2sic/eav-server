using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.DataSources.Configuration;
using ToSic.Eav.DataSourceTests.ExternalData;

namespace ToSic.Eav.DataSourceTests.Caches
{
    [TestClass]
    public class QuickCachesTest
    {

        [TestMethod]
        public void QuickCache_AddListAndCheckIfIn()
        {
            const string ItemToFilter = "1023";
            var ds = CreateFilterForTesting(100, ItemToFilter);

            //var cache = ds.Root;
            var listCache = new ListCache(null);// cache.Lists;// as IListCache;
            Assert.IsFalse(listCache.Has(ds.CacheFullKey), "Should not have it in cache yet");

            // manually add to cache
            listCache.Set(ds[Constants.DefaultStreamName]);
            Assert.IsTrue(listCache.Has(ds.CacheFullKey + "|Default"), "Should have it in cache now");
            Assert.IsTrue(listCache.Has(ds[Constants.DefaultStreamName]), "Should also have the DS default");
            
            Assert.IsTrue(listCache.Has(ds[Constants.DefaultStreamName]), "should have it by stream as well");
            

            // Try to auto-retrieve 
            var cached = listCache.Get(ds.CacheFullKey + "|Default").List;

            Assert.AreEqual(1, cached.Count());

            cached = listCache.Get(ds[Constants.DefaultStreamName]).List;
            Assert.AreEqual(1, cached.Count());

            var lci = listCache.Get(ds.CacheFullKey);
            Assert.AreEqual(null, lci, "Cached should be null because the name isn't correct");

            lci = listCache.Get(ds[Constants.DefaultStreamName]);
            Assert.AreNotEqual(null, lci, "Cached should be found because usin stream instead of name");

            cached = listCache.Get(ds[Constants.DefaultStreamName]).List;
            Assert.AreEqual(1, cached.Count());

        }

        [TestMethod]
        public void QuickCache_AddAndWaitForReTimeout()
        {
            const string ItemToFilter = "1027";
            var ds = CreateFilterForTesting(100, ItemToFilter);

            var listCache = new ListCache(null);// ds.Root.Lists; //as IListCache;
            (listCache as ListCache).DefaultDuration = 1;
            Assert.IsFalse(listCache.Has(ds.CacheFullKey), "Should not have it in cache yet");

            listCache.Set(ds.CacheFullKey, ds.Immutable, ds.CacheTimestamp);
            Assert.IsTrue(listCache.Has(ds.CacheFullKey), "Should have it in cache now");

            Thread.Sleep(400);
            Assert.IsTrue(listCache.Has(ds.CacheFullKey), "Should STILL be in cache");

            Thread.Sleep(601);
            Assert.IsFalse(listCache.Has(ds.CacheFullKey), "Should NOT be in cache ANY MORE");
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
