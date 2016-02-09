﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav.UnitTests.DataSources
{
    [TestClass]
    public class CacheAllStreams_Test
    {
        private string FilterIdForManyTests = "1067";
        private string AlternateIdForAlternateTests = "1069";

        [TestMethod]
        public void CacheAllStreams_CheckNotInBeforeAndInAfterwards()
        {
            var filtered = CreateFilterForTesting(100, FilterIdForManyTests);

            var cacher = CreateCacheDS(filtered);

            Assert.AreEqual("DataTableDataSource-NoGuid&ContentType=Person>EntityIdFilter-NoGuid&EntityIds=1067", filtered.CacheFullKey);

            // check if in cache - shouldn't be yet
            Assert.IsFalse(cacher.Cache.ListHas(cacher.Out[Constants.DefaultStreamName]),
                "Should not be in yet");

            var y = cacher.LightList; // not it should get in

            // check again, should be in
            Assert.IsTrue(cacher.Cache.ListHas(cacher.Out[Constants.DefaultStreamName]),
                "Should be in now");
        }

        [TestMethod]
        public void CacheAllStreams_CheckInWithIdenticalNewSetup()
        {
            var filtered = CreateFilterForTesting(100, FilterIdForManyTests);

            var cacher = CreateCacheDS(filtered);

            // Should already be in - even though it may be an old copy
            Assert.IsTrue(cacher.Cache.ListHas(cacher.Out[Constants.DefaultStreamName]),
                "Should be in because the previous test already added it");

            var y = cacher.LightList; // not it should get in

            Assert.AreEqual(1, y.Count(), "still has correct amount of items");
            Assert.AreEqual(1067, y.First().EntityId, "check correct entity id");
        }

        [TestMethod]
        public void CacheAllStreams_CheckInWithDifferentNewSetup()
        {
            var filtered = CreateFilterForTesting(100, AlternateIdForAlternateTests);

            CreateCacheDS(filtered);

            Assert.IsFalse((filtered.Cache as QuickCache).ListHas(filtered.Out[Constants.DefaultStreamName]),
                "Should not be in because the previous test added different item");

        }

        private CacheAllStreams CreateCacheDS(IDataSource filtered)
        {
            var cacher = new CacheAllStreams();
            cacher.Attach(filtered);
            cacher.ConfigurationProvider = filtered.ConfigurationProvider;
            return cacher;
        }

        [TestMethod]
        public void CacheAllStreams_CheckInWithLongerChain()
        {
            var filtered = CreateFilterForTesting(100, FilterIdForManyTests);
            var secondFilter = new EntityTypeFilter();
            secondFilter.Attach(filtered);
            secondFilter.TypeName = "Person";
            secondFilter.ConfigurationProvider = filtered.ConfigurationProvider;

            var cacher = CreateCacheDS(secondFilter);

            Assert.AreEqual("DataTableDataSource-NoGuid&ContentType=Person>EntityIdFilter-NoGuid&EntityIds=1067>EntityTypeFilter-NoGuid&TypeName=Person", secondFilter.CacheFullKey);

            Assert.IsFalse((cacher.Cache as QuickCache).ListHas(secondFilter.Out[Constants.DefaultStreamName]),
                "Should not be in because the previous test added a shorter key");
        }

        [TestMethod]
        public void CacheAllStreams_CheckSourceExpiry()
        {
            string uniqueIdsForThisTest = "1001,1005,1043";
            var filtered = CreateFilterForTesting(100, uniqueIdsForThisTest);
            var cacher = CreateCacheDS(filtered);

            // shouldn't be in cache et...
            Assert.IsFalse(cacher.Cache.ListHas(cacher.Out[Constants.DefaultStreamName]),
                "Should not be in because the previous test added a shorter key");

            // Get first list from direct query and from cache - compare. Should be same
            var originalList = cacher[Constants.DefaultStreamName].LightList;
            var listFromCache1 = cacher.Cache.ListGet(cacher.Out[Constants.DefaultStreamName]).LightList;
            Assert.AreEqual(listFromCache1, originalList, "Should be same list - right now");

            // Now wait 100 milliseconds, then repeat the process. Since the new source has another date-time, it should rebuild the cache
            Thread.Sleep(100);

            var laterTimeIdenticalData = CreateFilterForTesting(100, uniqueIdsForThisTest);
            var cache2 = CreateCacheDS(laterTimeIdenticalData);
            var listFromCache2 = cache2[Constants.DefaultStreamName].LightList;
            Assert.AreNotEqual(listFromCache2, originalList, "Second list sohuldn't be same because 100ms time difference in source");
        }

        [TestMethod]
        public void CacheAllStreams_IgnoreSourceExpiry()
        {
            string uniqueIdsForThisTest = "1001,1005,1043,1099";
            var filtered = CreateFilterForTesting(100, uniqueIdsForThisTest);
            var cacher = CreateCacheDS(filtered);

            // Get first list from direct query and from cache - compare. Should be same
            var originalList = cacher[Constants.DefaultStreamName].LightList;

            var cacheItem = cacher.Cache.ListGet(cacher.Out[Constants.DefaultStreamName]);
            Assert.IsNotNull(cacheItem, "should be not null, expected it to be in the cache");
            var listFromCache1 = cacheItem.LightList;
            Assert.AreEqual(listFromCache1, originalList, "Should be same list - right now");

            // Now wait 100 milliseconds, then repeat the process. Since the new source has another date-time, it should rebuild the cache
            Thread.Sleep(100);

            var laterTimeIdenticalData = CreateFilterForTesting(100, uniqueIdsForThisTest);
            var cache2 = CreateCacheDS(laterTimeIdenticalData);

            // special: tell cache2 to ignore time etc.
            cache2.RefreshOnSourceRefresh = false;

            var listFromCache2 = cache2[Constants.DefaultStreamName].LightList;
            Assert.AreEqual(listFromCache2, originalList, "Second list sohuldn't STILL be same because we ignore time difference in source");
        }


        [TestMethod]
        public void CacheAllStreams_ExpireBecauseOfTime()
        {
            string uniqueIdsForThisTest = "1001,1043,1099";
            var filtered = CreateFilterForTesting(100, uniqueIdsForThisTest);
            var cacher = CreateCacheDS(filtered);
            cacher.CacheDurationInSeconds = 1;

            // Get first list from direct query and from cache - compare. Should be same
            var originalList = cacher[Constants.DefaultStreamName].LightList;
            var listFromCache1 = cacher.Cache.ListGet(cacher.Out[Constants.DefaultStreamName]).LightList;
            Assert.AreEqual(listFromCache1, originalList, "Should be same list - right now");

            // Now wait 1 second, then repeat the process. Since the new source has another date-time, it should rebuild the cache
            Thread.Sleep(1001);

            // Create new identical filtered, and new cache-object to separate from original...
            filtered = CreateFilterForTesting(100, uniqueIdsForThisTest);
            var newCacher = CreateCacheDS(filtered);
            newCacher.CacheDurationInSeconds = 1;
            newCacher.RefreshOnSourceRefresh = false; // don't enforce this, otherwise it will automatically be a new cache anyhow
            var listFromCacheAfter1Second = newCacher[Constants.DefaultStreamName].LightList;
            Assert.AreNotEqual(listFromCacheAfter1Second, originalList, "Second list MUST be Different because 1 second passed");
        }

        public static EntityIdFilter CreateFilterForTesting(int testItemsInRootSource, string entityIdsValue)
        {
            var ds = DataTableDataSource_Test.GeneratePersonSourceWithDemoData(testItemsInRootSource, 1001);
            var filtered = new EntityIdFilter();
            filtered.ConfigurationProvider = ds.ConfigurationProvider;
            filtered.Attach(ds);
            filtered.EntityIds = entityIdsValue;
            return filtered;
        }
    }
}
