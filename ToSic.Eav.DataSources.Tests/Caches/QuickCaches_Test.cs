using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.Caches
{
    [TestClass]
    public class QuickCachesTest: TestBaseEavDataSource
    {
        public static ListCache GetTestListCache() => new ListCache();

        [TestMethod]
        public void QuickCache_AddListAndCheckIfIn()
        {
            const string ItemToFilter = "1023";
            var ds = CreateFilterForTesting(100, ItemToFilter);

            //var cache = ds.Root;
            var listCache = GetTestListCache();
            Assert.IsFalse(listCache.Has(ds.CacheFullKey), "Should not have it in cache yet");

            // manually add to cache
            listCache.Set(ds[Constants.DefaultStreamName]);
            Assert.IsTrue(listCache.Has(ds.CacheFullKey + "&Stream=Default"), "Should have it in cache now");
            Assert.IsTrue(listCache.Has(ds[Constants.DefaultStreamName]), "Should also have the DS default");
            
            Assert.IsTrue(listCache.Has(ds[Constants.DefaultStreamName]), "should have it by stream as well");
            

            // Try to auto-retrieve 
            var cached = listCache.Get(ds.CacheFullKey + "&Stream=Default").List;

            Assert.AreEqual(1, cached.Count());

            cached = listCache.Get(ds[Constants.DefaultStreamName]).List;
            Assert.AreEqual(1, cached.Count());

            var lci = listCache.Get(ds.CacheFullKey);
            Assert.AreEqual(null, lci, "Cached should be null because the name isn't correct");

            lci = listCache.Get(ds[Constants.DefaultStreamName]);
            Assert.AreNotEqual(null, lci, "Cached should be found because using stream instead of name");

            cached = listCache.Get(ds[Constants.DefaultStreamName]).List;
            Assert.AreEqual(1, cached.Count());

        }

        [TestMethod]
        public void QuickCache_AddAndWaitForReTimeout()
        {
            const string ItemToFilter = "1027";
            var ds = CreateFilterForTesting(100, ItemToFilter);

            var listCache = GetTestListCache();
            listCache.DefaultDuration = 1;
            Assert.IsFalse(listCache.Has(ds.CacheFullKey), "Should not have it in cache yet");

            listCache.Set(ds.CacheFullKey, ds.ListForTests().ToImmutableArray(), ds.CacheTimestamp);
            Assert.IsTrue(listCache.Has(ds.CacheFullKey), "Should have it in cache now");

            Thread.Sleep(400);
            Assert.IsTrue(listCache.Has(ds.CacheFullKey), "Should STILL be in cache");

            Thread.Sleep(601);
            Assert.IsFalse(listCache.Has(ds.CacheFullKey), "Should NOT be in cache ANY MORE");
        }




        public EntityIdFilter CreateFilterForTesting(int testItemsInRootSource, string entityIdsValue)
        {
            var ds = new DataTablePerson(this).Generate(testItemsInRootSource, 1001);
            var filtered = CreateDataSource<EntityIdFilter>(ds.Configuration.LookUpEngine);
            filtered.AttachForTests(ds);
            filtered.EntityIds = entityIdsValue;
            return filtered;
        }
    }
}
