using System;
using ToSic.Eav.DataSources.Caching.CacheInfo;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// Just a wrapper-class, so systems can differentiate between live and deferred streams.
    /// The goal is to provide a more optimized check if it should even look for the internal stream
    /// And otherwise just use the cache.
    /// </summary>
    [PrivateApi]
    public class DataStreamWithCustomCaching: DataStream
    {
        public DataStreamWithCustomCaching(Func<ICacheInfo> cacheInfoDelegate, IDataSource source, string name, GetImmutableListDelegate listDelegate, bool enableAutoCaching) 
            : base(source, name, listDelegate, enableAutoCaching)
        {
            CacheInfoDelegate = cacheInfoDelegate;
        }

        /// <summary>
        /// This will get a CacheInfo if ever needed - but as long as not needed, won't run
        /// </summary>
        public readonly Func<ICacheInfo> CacheInfoDelegate;

        /// <summary>
        /// The Cache-Suffix helps to keep these streams separate in case the original stream also says it caches
        /// Because then they would have the same cache-key, and that would cause trouble
        /// </summary>
        public override DataStreamCacheStatus Caching 
            => _cachingInternal ?? (_cachingInternal = new DataStreamCacheStatus(CacheInfoDelegate.Invoke(), Source, Name + "!Deferred"));
        private DataStreamCacheStatus _cachingInternal;
    }
}
