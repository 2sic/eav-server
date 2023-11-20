using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSource.Caching
{
    [PrivateApi]
    internal class DataSourceCacheService: ServiceBase, IDataSourceCacheService
    {
        public const int MaxRecursions = 100;
        private const string ErrRecursions = "too many recursions on UnCache";

        public DataSourceCacheService(IListCacheSvc listCache) : base("Ds.CchSvc")
        {
            ConnectServices(
                ListCache = listCache
            );
        }

        public IListCacheSvc ListCache { get; }


        public bool UnCache(int recursion, IDataSource dataSource, bool cascade = false, IReadOnlyDictionary<string, IDataStream> streams = default)
        {
            var l = Log.Fn<bool>($"{cascade} - on {dataSource.GetType().Name}, {nameof(recursion)}: {recursion}");
            if (recursion > MaxRecursions) throw l.Ex(new ArgumentOutOfRangeException(nameof(recursion), ErrRecursions));

            var streamDic = streams ?? dataSource.In;
            
            if (!streamDic.Any()) 
                return l.ReturnFalse("No streams found to clear");

            foreach (var stream in streamDic)
                UnCache(recursion + 1, stream.Value, cascade);

            if (dataSource is ICacheAlsoAffectsOut)
            {
                l.A("Also clear the Out");
                foreach (var stream in dataSource.Out)
                    UnCache(recursion + 1, stream.Value, cascade);
            }

            if (dataSource is IDataSourceReset reset)
            {
                l.A("Also reset the DataSource");
                reset.Reset();
            }

            return l.ReturnTrue();
        }

        public bool UnCache(int recursion, IDataStream stream, bool cascade = false)
        {
            var l = Log.Fn<bool>($"Stream: {stream.Name}, {nameof(cascade)}:{cascade}, {nameof(recursion)}: {recursion}");
            if (recursion > MaxRecursions) throw l.Ex(new ArgumentOutOfRangeException(nameof(recursion), ErrRecursions));

            stream.ResetStream();
            l.A("kill in list-cache");
            Remove(stream);
            if (!cascade) return l.ReturnTrue();
            l.A("tell upstream source to flush as well");
            UnCache(recursion + 1, stream.Source, true);
            return l.ReturnTrue();
        }

        /// <summary>
        /// Remove an item from the list cache using a data-stream key
        /// </summary>
        /// <param name="dataStream">the data stream, which can provide it's cache-key</param>
        public void Remove(IDataStream dataStream) => Remove(DataSourceListCache.CacheKey(dataStream));

        /// <summary>
        /// Remove an item from the list-cache using the string-key
        /// </summary>
        /// <param name="key">the identifier in the cache</param>
        public void Remove(string key) => Log.Do(parameters: key, action: () => MemoryCache.Default.Remove(key));
    }
}
