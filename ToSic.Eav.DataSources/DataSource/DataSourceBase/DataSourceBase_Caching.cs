using System;
using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSource;

partial class DataSourceBase
{
    #region Caching stuff

    /// <inheritdoc />
    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public List<string> CacheRelevantConfigurations { get; } = new();


    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public ICacheKeyManager CacheKey => _cacheKey ??= new CacheKey(this);
    private CacheKey _cacheKey;

    /// <inheritdoc />
    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public virtual string CachePartialKey => CacheKey.CachePartialKey;

    /// <inheritdoc />
    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
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