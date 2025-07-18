﻿using ToSic.Sys.Caching.Keys;

namespace ToSic.Eav.DataSource.Sys.Caching;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class CacheKey(IDataSource dataSource) : ICacheKeyManager
{
    [ShowApiWhenReleased(ShowApiMode.Never)] 
    public readonly IDataSource DataSource = dataSource;


    /// <inheritdoc />
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public virtual string CachePartialKey
    {
        get
        {
            // Assemble the partial key
            // If this item has a guid that it's a configured part which always has this unique guid; then use that
            var key = DataSource.Name + ":" + (DataSource.Guid != Guid.Empty
                ? DataSource.Guid.ToString()
                : "NoGuid");

            // Important to check configuration first - to ensure all tokens are resolved to the resulting parameters
            DataSource.Configuration.Parse();

            // note: whenever a item has filter-parameters, these should be part of the key as well...

            return DataSource.CacheRelevantConfigurations
                .Aggregate(key, (current, configName) => 
                    current + "&" + configName + "=" + DataSource.Configuration.Values[configName]);
        }
    }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    [field: AllowNull, MaybeNull]
    public virtual string CacheFullKey => field ??= string.Join(">", SubKeys.Distinct());


    /// <summary>
    /// make sure we don't re-create many keys, of if some streams have the same DataSource, only get the key once
    /// </summary>
    /// <returns></returns>
    private List<IDataSource> UniqueSources()
    {
        // Note: doing extra careful null checks here, as this is used in the cache key, and if it fails, it will break the cache
        if (DataSource == null!)
            return [];

        if (DataSource.In == null! || DataSource.In.Count == 0)
            return [];

        return DataSource.In
            .Select(pairs => pairs.Value?.Source)
            .Where(stream => stream != null)
            .Cast<IDataSource>()
            .Distinct()
            .ToList();
    }


    [ShowApiWhenReleased(ShowApiMode.Never)]
    [field: AllowNull, MaybeNull]
    public string[] SubKeys
    {
        get
        {
            if (field != null)
                return field;
            var keys = UniqueSources()
                .SelectMany(inStream => inStream.CacheKey.SubKeys)
                .Append(CachePartialKey)
                .ToList();
            field = [.. keys];
            return field;
        }
    }
}