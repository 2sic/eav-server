using ToSic.Eav.DataSource.Internal.Caching;

namespace ToSic.Eav.DataSource;

public static class ListCacheSvcTestAccessors
{
    public static bool HasStreamTac(this IListCacheSvc listCache, string key)
        => listCache.HasStream(key);

    public static bool HasStreamTac(this IListCacheSvc listCache, IDataStream dataStream)
        => listCache.HasStream(dataStream);

    public static ListCacheItem GetTac(this IListCacheSvc listCache, string key)
        => listCache.Get(key);

    public static ListCacheItem GetTac(this IListCacheSvc listCache, IDataStream dataStream)
        => listCache.Get(dataStream);
}