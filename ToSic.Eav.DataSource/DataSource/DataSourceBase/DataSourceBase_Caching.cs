using ToSic.Eav.DataSource.Sys.Caching;
using ToSic.Sys.Caching.Keys;

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
    [field: AllowNull, MaybeNull]
    public ICacheKeyManager CacheKey => field ??= new CacheKey(this);

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