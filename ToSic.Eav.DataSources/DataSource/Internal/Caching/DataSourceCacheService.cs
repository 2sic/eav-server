using ToSic.Eav.Caching;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.DataSource.Internal.Caching;

[PrivateApi]
internal class DataSourceCacheService(IListCacheSvc listCache, MemoryCacheService memoryCacheService) : ServiceBase("Ds.CchSvc", connect: [ listCache, memoryCacheService ]), IDataSourceCacheService
{
    public const int MaxRecursions = 100;
    private const string ErrRecursions = "too many recursions on UnCache";
    public IListCacheSvc ListCache => listCache;

    /// <inheritdoc />
    public bool Flush(IDataStream stream, bool cascade = false) => FlushStream(stream, 0, cascade);


    /// <inheritdoc />
    public bool FlushAll()
    {
        var l = Log.Fn<bool>();
        var keys = DataSourceListCache.LoadLocks.Keys.ToList();
        foreach (var key in keys) FlushKey(key);
        DataSourceListCache.LoadLocks.Clear();
        return l.ReturnTrue();
    }

    /// <inheritdoc />
    public bool Flush(string key)
    {
        var l = Log.Fn<bool>(key);
        FlushKey(key);
        return l.ReturnTrue();
    }

    /// <inheritdoc />
    public bool Flush(IDataSource dataSource, bool cascade = false) => FlushDs(dataSource, 0, cascade);

    private bool FlushDs(IDataSource dataSource, int recursion, bool cascade = false)
    {
        var l = Log.Fn<bool>($"{cascade} - on {dataSource.GetType().Name}, {nameof(recursion)}: {recursion}");
        if (recursion > MaxRecursions) throw l.Ex(new ArgumentOutOfRangeException(nameof(recursion), ErrRecursions));

        var result = FlushStreamList(dataSource.In, recursion, cascade);

        if (dataSource is ICacheAlsoAffectsOut)
        {
            l.A("Also clear the Out");
            FlushStreamList(dataSource.Out, recursion, cascade);
        }

        if (dataSource is IDataSourceReset reset)
        {
            l.A("Also reset the DataSource");
            reset.Reset();
        }

        return l.Return(result);
    }


    private bool FlushStreamList(IReadOnlyDictionary<string, IDataStream> streams, int recursion, bool cascade)
    {
        var l = Log.Fn<bool>($"Streams: {streams.Count}");
        if (streams.SafeNone()) 
            return l.ReturnFalse("No streams found to clear");

        foreach (var stream in streams)
            FlushStream(stream.Value, recursion + 1, cascade);

        return l.ReturnTrue();
    }


    private bool FlushStream(IDataStream stream, int recursion, bool cascade = false)
    {
        var l = Log.Fn<bool>($"Stream: {stream.Name}, {nameof(cascade)}:{cascade}, {nameof(recursion)}: {recursion}");
        if (recursion > MaxRecursions) throw l.Ex(new ArgumentOutOfRangeException(nameof(recursion), ErrRecursions));

        stream.ResetStream();
        l.A("kill in list-cache");
        FlushStream(stream);
        if (!cascade) return l.ReturnTrue();
        l.A("tell upstream source to flush as well");
        FlushDs(stream.Source, recursion + 1, true);
        return l.ReturnTrue();
    }

    /// <summary>
    /// Remove an item from the list cache using a data-stream key
    /// </summary>
    /// <param name="dataStream">the data stream, which can provide it's cache-key</param>
    private void FlushStream(IDataStream dataStream) => FlushKey(DataSourceListCache.CacheKey(dataStream));

    /// <summary>
    /// Remove an item from the list-cache using the string-key
    /// </summary>
    /// <param name="key">the identifier in the cache</param>
    private void FlushKey(string key) => Log.Do(parameters: key, action: () => memoryCacheService.Remove(key));
}