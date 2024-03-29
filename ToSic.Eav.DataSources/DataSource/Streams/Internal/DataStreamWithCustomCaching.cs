﻿using ToSic.Eav.DataSource.Internal.Caching;

namespace ToSic.Eav.DataSource.Streams.Internal;

/// <inheritdoc />
/// <summary>
/// Just a wrapper-class, so systems can differentiate between live and deferred streams.
/// The goal is to provide a more optimized check if it should even look for the internal stream
/// And otherwise just use the cache.
/// </summary>
[PrivateApi]
internal class DataStreamWithCustomCaching: DataStream
{
    public DataStreamWithCustomCaching(LazySvc<IDataSourceCacheService> cache, Func<ICacheInfo> cacheInfoDelegate, IDataSource source, string name, Func<IImmutableList<IEntity>> listDelegate, 
        bool enableAutoCaching, string scope)
        : base(cache, source, name, listDelegate, enableAutoCaching)
    {
        CacheInfoDelegate = cacheInfoDelegate;
        Scope = scope;
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
        => _cachingInternal ??= new(CacheInfoDelegate.Invoke(), Source, Name + "!Deferred");
    private DataStreamCacheStatus _cachingInternal;
}