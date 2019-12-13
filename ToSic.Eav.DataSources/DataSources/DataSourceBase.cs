using System;
using System.Collections.Generic;
using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.DataSources.Configuration;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// The base class, which should always be inherited. Already implements things like Get One / Get many, Caching and a lot more.
    /// </summary>
    [PublicApi]
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
            Configuration.Add(key, mask);
            if (cacheRelevant)
                CacheRelevantConfigurations.Add(key);
        }

        [PrivateApi]
        protected CacheKey CacheKey => _cacheKey ?? (_cacheKey = new CacheKey(this));
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
        public virtual IDictionary<string, IDataStream> Out { get; protected internal set; } = new Dictionary<string, IDataStream>();

        /// <inheritdoc />
        public IDataStream this[string outName] => Out[outName];

        /// <inheritdoc />
        public IEnumerable<IEntity> List => Out[Constants.DefaultStreamName].List;

        [PrivateApi]
        [Obsolete("deprecated since 2sxc 9.8 / eav 4.5 - use List")]
        public IEnumerable<IEntity> LightList => List;

        public DataSourceConfiguration ConfigTemp => _config ?? (_config = new DataSourceConfiguration(this));
        private DataSourceConfiguration _config;

        /// <inheritdoc />
        public ILookUpEngine ConfigurationProvider { get; protected internal set; }

        /// <inheritdoc />
        public IDictionary<string, string> Configuration { get; internal set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        [PrivateApi]
        protected internal bool ConfigurationIsLoaded;


        /// <summary>
        /// Make sure that configuration-parameters have been parsed (tokens resolved)
        /// but do it only once (for performance reasons)
        /// </summary>
        [PrivateApi]
		protected internal virtual void EnsureConfigurationIsLoaded()
        {
            if (ConfigurationIsLoaded)
                return;

            // Ensure that we have a configuration-provider (not always the case, but required)
            if (ConfigurationProvider == null)
                throw new Exception($"No ConfigurationProvider configured on this data-source. Cannot run {nameof(EnsureConfigurationIsLoaded)}");

            // construct a property access for in, use it in the config provider
            var instancePAs = new Dictionary<string, ILookUp> { { "In".ToLower(), new LookUpInDataTarget(this) } };
            Configuration = ConfigurationProvider.LookUp(Configuration, instancePAs);
            ConfigurationIsLoaded = true;
        }

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

        ///// <inheritdoc />
        //public virtual bool Ready => In[Constants.DefaultStreamName].Source != null 
        //                             && In[Constants.DefaultStreamName].Source.Ready;

        #region API to build items, in case this data source generates items

        //private const string UnspecifiedType = "unspecified";

        ///// <summary>
        ///// Convert a dictionary of values into an entity
        ///// </summary>
        ///// <param name="values">dictionary of values</param>
        ///// <param name="titleField">which field should be access if every something wants to know the title of this item</param>
        ///// <param name="typeName">an optional type-name - usually not needed, defaults to "unspecified"</param>
        ///// <param name="id">an optional id for this item, defaults to 0</param>
        ///// <param name="guid">an optional guid for this item, defaults to empty guid</param>
        ///// <param name="modified"></param>
        ///// <param name="appId">optional app id for this item, defaults to the current app</param>
        ///// <returns></returns>
        //[PrivateApi]
        //protected IEntity AsEntity(Dictionary<string, object> values,
        //    string titleField = null,
        //    string typeName = UnspecifiedType,
        //    int id = 0,
        //    Guid? guid = null,
        //    DateTime? modified = null,
        //    int? appId = null)
        //    => Build.Entity(appId ?? AppId, id, values, titleField: titleField, typeName: typeName, guid: guid, modified: modified);
            //=> new Data.Entity(appId ?? AppId, id, ContentTypeBuilder.Fake(typeName), values, titleField, modified, entityGuid: guid);



        ///// <summary>
        ///// Convert a list of value-dictionaries dictionary into a list of entities
        ///// this assumes that the entities don't have an own id or guid, 
        ///// otherwise you should use the single-item overload
        ///// </summary>
        ///// <param name="itemValues">list of value-dictionaries</param>
        ///// <param name="titleField">which field should be access if every something wants to know the title of this item</param>
        ///// <param name="typeName">an optional type-name - usually not needed, defaults to "unspecified"</param>
        ///// <param name="appId">optional app id for this item, defaults to the current app</param>
        ///// <returns></returns>
        //[PrivateApi]
        //protected IEnumerable<IEntity> AsEntity(IEnumerable<Dictionary<string, object>> itemValues,
        //    string titleField = null,
        //    string typeName = UnspecifiedType,
        //    int? appId = null)
        //    => itemValues.Select(i => AsEntity(i, titleField, typeName, 0, null, appId: appId));

        #endregion

    }
}