using System;
using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.DataSources.Configuration;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// The base class, which should always be inherited. Already implements things like Get One / Get many, Caching and a lot more.
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public abstract class DataSourceBase : HasLog, IDataSource, IDataTarget
    {
        /// <summary>
        /// The name to be used in logging. It's set in the code, and then used to initialize the logger. 
        /// </summary>
        [PrivateApi]
        public abstract string LogId { get; }

        /// <summary>
        /// Constructor - must be without parameters, otherwise the DI can't construct it.
        /// </summary>
        protected DataSourceBase() : base("DS.Base") { }

        /// <inheritdoc />
        public string Name => GetType().Name;

        #region Caching stuff

        /// <inheritdoc />
        public List<string> CacheRelevantConfigurations { get; set; } = new List<string>();

        [PrivateApi]
        protected void ConfigMask(string key, string mask, bool cacheRelevant = true)
        {
            Configuration.Values.Add(key, mask);
            if (cacheRelevant)
                CacheRelevantConfigurations.Add(key);
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
        public virtual int AppId { get; set; }

        /// <inheritdoc />
        public virtual int ZoneId { get; set; }

        /// <inheritdoc />
        public Guid Guid { get; set; }

        /// <inheritdoc />
        public IDictionary<string, IDataStream> In { get; internal set; } = new Dictionary<string, IDataStream>();

        /// <inheritdoc />
        public virtual IDictionary<string, IDataStream> Out { get; protected internal set; } = new StreamDictionary();

        /// <inheritdoc />
        public IDataStream this[string outName] => Out[outName];

        /// <inheritdoc />
        public IEnumerable<IEntity> List => Out[Constants.DefaultStreamName].List;

        //[PrivateApi]
        //[Obsolete("deprecated since 2sxc 9.8 / eav 4.5 - use List")]
        //public IEnumerable<IEntity> LightList => List;

        public DataSourceConfiguration Configuration => _config ?? (_config = new DataSourceConfiguration(this));
        private DataSourceConfiguration _config;





        #region various Attach-In commands
        /// <inheritdoc />
        public void Attach(IDataSource dataSource)
        {
            // ensure list is blank, otherwise we'll have name conflicts when replacing a source
            if (In.Count > 0)
                In.Clear();
            foreach (var dataStream in dataSource.Out)
                In.Add(dataStream.Key, dataStream.Value);
        }


        /// <inheritdoc />
        public void Attach(string streamName, IDataSource dataSource) 
            => Attach(streamName, dataSource[Constants.DefaultStreamName]);

        /// <inheritdoc />
        public void Attach(string streamName, IDataStream dataStream)
        {
            if (In.ContainsKey(streamName))
                In.Remove(streamName);

            In.Add(streamName, dataStream);
        }

        #endregion

        #region Various provide-out commands - all PrivateApi

        [PrivateApi]
        public void Provide(GetIEnumerableDelegate getList) 
            => Provide(Constants.DefaultStreamName, getList);

        [PrivateApi]
        public void Provide(string name, GetIEnumerableDelegate getList) 
            => Out.Add(name, new DataStream(this, name, getList));

        #endregion

        #region Internals (Ready)


        /// <inheritdoc />
        [PrivateApi]
        public bool OutIsDynamic { get; protected set; } = false;

        #endregion

        /// <inheritdoc />
        public void PurgeList(bool cascade = false)
        {
            foreach (var stream in In)
                stream.Value.PurgeList(cascade);
        }

    }
}