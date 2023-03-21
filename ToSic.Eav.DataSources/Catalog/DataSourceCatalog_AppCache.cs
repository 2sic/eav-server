using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using ToSic.Eav.Caching.CachingMonitors;

namespace ToSic.Eav.DataSources.Catalog
{
    public partial class DataSourceCatalog
    {
        /// <summary>
        /// A cache of all DataSource Types - initialized upon first access ever, then static cache.
        /// </summary>
        public static string AppCacheKey(int appId) => $"{appId}";

        private static MemoryCache AppCache => MemoryCache.Default;

        public List<DataSourceInfo> Get(int appId) => AppCache[AppCacheKey(appId)] as List<DataSourceInfo>;

        public void UpdateAppCache(int appId, IEnumerable<Type> appDataSources, /*IFeaturesInternal features,*/ string physicalPath, CacheEntryUpdateCallback updateCallback = null)
        {
            try
            {
                var expiration = new TimeSpan(1, 0, 0);
                var policy = new CacheItemPolicy { SlidingExpiration = expiration };

                // flush cache when any feature is changed
                //policy.ChangeMonitors.Add(new FeaturesResetMonitor(features));

                if (Directory.Exists(physicalPath)) 
                    policy.ChangeMonitors.Add(new FolderChangeMonitor(new List<string> { physicalPath }));

                if (updateCallback != null)
                    policy.UpdateCallback = updateCallback;

                var data = (appDataSources ?? new List<Type>())
                    .Select(t => new DataSourceInfo(t, false))
                    .ToList();

                AppCache.Set(new CacheItem(AppCacheKey(appId), data), policy);
            }
            catch
            {
                /* ignore for now */
            }
        }
    }
}
