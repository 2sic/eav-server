using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.ValueProvider;
using ICache = ToSic.Eav.DataSources.Caches.ICache;

namespace ToSic.Eav.DataSources
{
    /// <inheritdoc cref="IDataSource" />
    /// <inheritdoc cref="IDataTarget" />
    /// <summary>
    /// The base class, which should always be inherited. Already implements things like Get One / Get many, etc. 
    /// also maintains default User-May-Do-Edit/Sort etc. values
    /// </summary>
    public abstract class BaseDataSource : HasLog, IDataSource, IDataTarget
    {
        public new abstract string LogId { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected BaseDataSource() : base("DS.Base") { }

        /// <inheritdoc />
        /// <summary>
        /// Name of this data source - mainly to aid debugging
        /// </summary>
        public string Name => GetType().Name;

        #region Caching stuff

        /// <inheritdoc />
        public List<string> CacheRelevantConfigurations { get; set; } = new List<string>();

        protected void ConfigMask(string key, string mask, bool cacheRelevant = true)
        {
            Configuration.Add(key, mask);
            if (cacheRelevant)
                CacheRelevantConfigurations.Add(key);
        }

        /// <inheritdoc />
        public virtual string CachePartialKey
        {
            get
            {
                // Assemble the partial key
                // If this item has a guid thet it's a configured part which always has this unique guid; then use that
                var key = DataSourceGuid != Guid.Empty 
                    ? Name + DataSourceGuid 
                    : Name + "-NoGuid";

                // Important to check configuration first - to ensure all tokens are resolved to the resulting parameters
                EnsureConfigurationIsLoaded();

                // note: whenever a item has filter-parameters, these should be part of the key as well...

                return CacheRelevantConfigurations
                    .Aggregate(key, (current, configName) => current + "&" + configName + "=" + Configuration[configName]);
            }
        }

        public ICache Cache => DataSource.GetCache(ZoneId, AppId);

        public virtual string CacheFullKey
        {
            get
            {
                var fullKey = "";

                // If there is an upstream, use that as the leading part of the id
                if (In.ContainsKey(Constants.DefaultStreamName) && In[Constants.DefaultStreamName] != null)
                    fullKey += In[Constants.DefaultStreamName].Source.CacheFullKey + ">";

                // add current key
                fullKey += CachePartialKey;
                return fullKey;
            }
        }


        /// <inheritdoc />
        public virtual long CacheTimestamp 
            => In.ContainsKey(Constants.DefaultStreamName) && In[Constants.DefaultStreamName].Source != null
                ? In[Constants.DefaultStreamName].Source.CacheTimestamp
                : DateTime.Now.Ticks; // if no relevant up-stream, just return now!

        public virtual bool CacheChanged(long prevCacheTimestamp) => 
            !In.ContainsKey(Constants.DefaultStreamName) 
            || In[Constants.DefaultStreamName].Source == null 
            || In[Constants.DefaultStreamName].Source.CacheChanged(prevCacheTimestamp);

        #endregion

        /// <inheritdoc />
        public virtual int AppId { get; set; }

        /// <inheritdoc />
        public virtual int ZoneId { get; set; }

        public Guid DataSourceGuid { get; set; }

        public IDictionary<string, IDataStream> In { get; internal set; } = new Dictionary<string, IDataStream>();
        public virtual IDictionary<string, IDataStream> Out { get; protected internal set; } = new Dictionary<string, IDataStream>();

        public IDataStream this[string outName] => Out[outName];

        public IEnumerable<IEntity> List => Out[Constants.DefaultStreamName].List;

        [Obsolete("deprecated since 2sxc 9.8 / eav 4.5 - use List instead")]
        public IEnumerable<IEntity> LightList => List;


        public IValueCollectionProvider ConfigurationProvider { get; protected internal set; }
        public IDictionary<string, string> Configuration { get; internal set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        protected internal bool ConfigurationIsLoaded;


        // protected internal Dictionary<string, IValueProvider> Settings 
        /// <summary>
        /// Make sure that configuration-parameters have been parsed (tokens resolved)
        /// but do it only once (for performance reasons)
        /// </summary>
		protected internal virtual void EnsureConfigurationIsLoaded()
        {
            if (ConfigurationIsLoaded)
                return;

            // Ensure that we have a configuration-provider (not always the case, but required)
            if (ConfigurationProvider == null)
                throw new Exception("No ConfigurationProvider configured on this data-source. Cannot EnsureConfigurationIsLoaded");

            // construct a property access for in, use it in the config provider
            var instancePAs = new Dictionary<string, IValueProvider> { { "In".ToLower(), new DataTargetValueProvider(this) } };
            ConfigurationProvider.LoadConfiguration(Configuration, instancePAs);
            ConfigurationIsLoaded = true;
        }

        #region various Attach-In commands
        /// <inheritdoc />
        /// <summary>
        /// Attach specified DataSource to In
        /// </summary>
        /// <param name="dataSource">DataSource to attach</param>
        public void Attach(IDataSource dataSource)
        {
            // ensure list is blank, otherwise we'll have name conflicts when replacing a source
            if (In.Count > 0)
                In.Clear();
            foreach (var dataStream in dataSource.Out)
                In.Add(dataStream.Key, dataStream.Value);
        }


        public void Attach(string streamName, IDataSource dataSource) 
            => Attach(streamName, dataSource[Constants.DefaultStreamName]);

        public void Attach(string streamName, IDataStream dataStream)
        {
            if (In.ContainsKey(streamName))
                In.Remove(streamName);

            In.Add(streamName, dataStream);
        }

        #endregion

        #region Various provide-out commands

        public void Provide(GetIEnumerableDelegate getList) 
            => Provide(Constants.DefaultStreamName, getList);

        public void Provide(string name, GetIEnumerableDelegate getList) 
            => Out.Add(name, new DataStream(this, name, getList));

        #endregion

        #region Internals (Ready)

        /// <inheritdoc />
        public virtual bool Ready => In[Constants.DefaultStreamName].Source != null && In[Constants.DefaultStreamName].Source.Ready;

        public bool TempUsesDynamicOut { get; protected set; } = false;

        #endregion


        #region API to build items, in case this data source generates items

        private const string UnspecifiedType = "unspecified";

        /// <summary>
        /// Convert a dictionary of values into an entity
        /// </summary>
        /// <param name="values">dictionary of values</param>
        /// <param name="titleField">which field should be access if every something wants to know the title of this item</param>
        /// <param name="typeName">an optional type-name - usually not needed, defaults to "unspecified"</param>
        /// <param name="id">an optional id for this item, defaults to 0</param>
        /// <param name="guid">an optional guid for this item, defaults to empty guid</param>
        /// <param name="modified"></param>
        /// <param name="appId">optional app id for this item, defaults to the current app</param>
        /// <returns></returns>
        protected IEntity AsEntity(Dictionary<string, object> values,
            string titleField = null,
            string typeName = UnspecifiedType,
            int id = 0,
            Guid? guid = null,
            DateTime? modified = null,
            int? appId = null)
            => new Data.Entity(appId ?? AppId, id, ContentTypeBuilder.Fake(typeName), values, titleField, modified, entityGuid: guid);



        /// <summary>
        /// Convert a list of value-dictionaries dictionary into a list of entities
        /// this assumes that the entities don't have an own id or guid, 
        /// otherwise you should use the single-item overload
        /// </summary>
        /// <param name="itemValues">list of value-dictionaries</param>
        /// <param name="titleField">which field should be access if every something wants to know the title of this item</param>
        /// <param name="typeName">an optional type-name - usually not needed, defaults to "unspecified"</param>
        /// <param name="appId">optional app id for this item, defaults to the current app</param>
        /// <returns></returns>
        protected IEnumerable<IEntity> AsEntity(IEnumerable<Dictionary<string, object>> itemValues,
            string titleField = null,
            string typeName = UnspecifiedType,
            int? appId = null)
            => itemValues.Select(i => AsEntity(i, titleField, typeName, 0, null, appId: appId));

        #endregion
    }
}