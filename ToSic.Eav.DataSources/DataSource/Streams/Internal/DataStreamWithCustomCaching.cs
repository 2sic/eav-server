using ToSic.Eav.DataSource.Internal.Caching;

namespace ToSic.Eav.DataSource.Streams.Internal;

/// <inheritdoc />
/// <summary>
/// Just a wrapper-class, so systems can differentiate between live and deferred streams.
/// The goal is to provide a more optimized check if it should even look for the internal stream
/// And otherwise just use the cache.
/// </summary>
[PrivateApi]
internal class DataStreamWithCustomCaching(
    LazySvc<IDataSourceCacheService> cache,
    Func<ICacheInfo> cacheInfoDelegate,
    IDataSource source,
    string name,
    Func<IImmutableList<IEntity>> listDelegate,
    bool enableAutoCaching,
    string scope)
    : DataStream(cache: cache, source: source, name: name, listDelegate: listDelegate,
        enableAutoCaching: enableAutoCaching, scope: scope)
{
    /// <summary>
    /// This will get a CacheInfo if ever needed - but as long as not needed, won't run
    /// </summary>
    public Func<ICacheInfo> CacheInfoDelegate { get; } = cacheInfoDelegate;

    /// <summary>
    /// The Cache-Suffix helps to keep these streams separate in case the original stream also says it caches
    /// Because then they would have the same cache-key, and that would cause trouble
    /// </summary>
    public override DataStreamCacheStatus Caching 
        => field ??= new(CacheInfoDelegate.Invoke(), Source, Name + "!Deferred");
}