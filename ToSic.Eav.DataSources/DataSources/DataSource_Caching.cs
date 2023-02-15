﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.DataSources.Caching;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSources
{
    public abstract partial class DataSource
    {
        #region Caching stuff

        /// <inheritdoc />
        [InternalApi_DoNotUse_MayChangeWithoutNotice]
        public List<string> CacheRelevantConfigurations { get; set; } = new List<string>();


        [PrivateApi]
        public ICacheKeyManager CacheKey => _cacheKey ?? (_cacheKey = new CacheKey(this));
        private CacheKey _cacheKey;

        /// <inheritdoc />
        public virtual string CachePartialKey => CacheKey.CachePartialKey;

        /// <inheritdoc />
        public virtual string CacheFullKey => CacheKey.CacheFullKey;

        /// <inheritdoc />
        public virtual long CacheTimestamp
            => In.ContainsKey(Constants.DefaultStreamName) && In[Constants.DefaultStreamName].Source != null
                ? In[Constants.DefaultStreamName].Source.CacheTimestamp
                : DateTime.Now.Ticks; // if no relevant up-stream, just return now!

        /// <inheritdoc />
        public virtual bool CacheChanged(long newCacheTimeStamp) =>
            !In.ContainsKey(Constants.DefaultStreamName)
            || In[Constants.DefaultStreamName].Source == null
            || In[Constants.DefaultStreamName].Source.CacheChanged(newCacheTimeStamp);

        #endregion


        /// <inheritdoc />
        public virtual void PurgeList(bool cascade = false) => Log.Do($"{cascade} - on {GetType().Name}", l =>
        {
            foreach (var stream in In)
                stream.Value.PurgeList(cascade);
            if (!In.Any()) l.A("No streams found to clear");
        });
    }
}