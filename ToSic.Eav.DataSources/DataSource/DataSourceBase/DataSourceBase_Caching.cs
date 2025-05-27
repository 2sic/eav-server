using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Lib.Caching.Keys;

namespace ToSic.Eav.DataSource;

partial class DataSourceBase
{
    #region Caching stuff

    /// <inheritdoc />
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public List<string> CacheRelevantConfigurations { get; } = [];


    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public ICacheKeyManager CacheKey => _cacheKey ??= new(this);
    private CacheKey _cacheKey;

    /// <inheritdoc />
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public virtual string CachePartialKey => CacheKey.CachePartialKey;

    /// <inheritdoc />
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public virtual string CacheFullKey => CacheKey.CacheFullKey;

    /// <inheritdoc />
    public virtual long CacheTimestamp
        => In.ContainsKey(DataSourceConstants.StreamDefaultName) && In[DataSourceConstants.StreamDefaultName].Source != null
            ? In[DataSourceConstants.StreamDefaultName].Source.CacheTimestamp
            : DateTime.Now.Ticks; // if no relevant up-stream, just return now!

    /// <inheritdoc />
    public virtual bool CacheChanged(long dependentTimeStamp) =>
        !In.ContainsKey(DataSourceConstants.StreamDefaultName)
        || In[DataSourceConstants.StreamDefaultName].Source == null
        || In[DataSourceConstants.StreamDefaultName].Source.CacheChanged(dependentTimeStamp);

    #endregion
}