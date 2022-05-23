using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.DataSources
{
    public abstract partial class DataSourceBase
    {
        #region Caching stuff

        /// <inheritdoc />
        [InternalApi_DoNotUse_MayChangeWithoutNotice]
        public List<string> CacheRelevantConfigurations { get; set; } = new List<string>();

        /// <summary>
        /// Add a value to the configuration list for later resolving tokens and using in Cache-Keys.
        /// </summary>
        /// <param name="key">The internal key to reference this value in the Configuration[Key] dictionary.</param>
        /// <param name="mask">The string containing [Tokens](xref:Abyss.Parts.LookUp.Tokens) which will be parsed to find the final value.</param>
        /// <param name="cacheRelevant">If this key should be part of the cache-key. Default is true. Set to false for parameters which don't affect the result or are confidential (like passwords)</param>
        [PublicApi]
        protected void ConfigMask(string key, string mask, bool cacheRelevant = true)
        {
            Configuration.Values.Add(key, mask);
            if (cacheRelevant)
                CacheRelevantConfigurations.Add(key);
        }

        [PrivateApi("WIP v12.10")]
        protected void ConfigMask(string keyAndMask, bool cacheRelevant = true)
        {
            // Key - in future it should detect if the keyAndMask has more than just the key, and extract the key
            var separator = keyAndMask.IndexOfAny(new[] { '|'}); //, '[' });
            var key = separator > 0 ? keyAndMask.Substring(0, separator) : keyAndMask;
            var mask = $"[Settings:{keyAndMask}]";
            ConfigMask(key, mask, cacheRelevant);
        }


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
        public virtual void PurgeList(bool cascade = false)
        {
            var callLog = Log.Call($"{cascade}", $"on {GetType().Name}");
            foreach (var stream in In)
                stream.Value.PurgeList(cascade);
            if (!In.Any()) Log.A("No streams found to clear");
            callLog("ok");
        }
    }
}