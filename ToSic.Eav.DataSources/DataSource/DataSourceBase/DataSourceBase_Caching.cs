﻿using System;
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
    public List<string> CacheRelevantConfigurations { get; } = new();


    [PrivateApi]
    public ICacheKeyManager CacheKey => _cacheKey ??= new CacheKey(this);
    private CacheKey _cacheKey;

    /// <inheritdoc />
    [PrivateApi]
    public virtual string CachePartialKey => CacheKey.CachePartialKey;

    /// <inheritdoc />
    [PrivateApi]
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


    ///// <inheritdoc />
    //[PrivateApi]
    //// TODO: MOVING TO DataSourceCacheService
    //public virtual void PurgeList(bool cascade = false)// => Log.Do($"{cascade} - on {GetType().Name}", l =>
    //{
    //    Services.DsCacheSvc.Value.UnCache(0,this, cascade);
    //    //foreach (var stream in In)
    //    //    stream.Value.PurgeList(cascade);
    //    //if (!In.Any()) l.A("No streams found to clear");
    //}//);
}