using System;
using System.Collections.Generic;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.ValueProvider;

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
        protected BaseDataSource() : base("DS.Base")
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Name of this data source - mainly to aid debugging
        /// </summary>
        public string Name => GetType().Name;

        #region Caching stuff

        /// <summary>
        /// List of items from the configuration which should be used for creating the cache-key
        /// </summary>
        public string[] CacheRelevantConfigurations { get; set; } = new string[0];

        /// <summary>
        /// Unique key-id for this specific part - without the full chain to the parents
        /// </summary>
        public virtual string CachePartialKey
        {
            get
            {
                var key = "";
                // Assemble the partial key
                // If this item has a guid thet it's a configured part which always has this unique guid; then use that
                if (DataSourceGuid != Guid.Empty)
                    key += Name + DataSourceGuid;
                else
                    key += Name + "-NoGuid";

                // Important to check configuration first - to ensure all tokens are resolved to the resulting parameters
                EnsureConfigurationIsLoaded();

                // note: whenever a item has filter-parameters, these should be part of the key as well...
                foreach (var configName in CacheRelevantConfigurations)
                    key += "&" + configName + "=" + Configuration[configName];

                return key;
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

        public virtual DateTime CacheLastRefresh
        {
            get
            {
                // try to return the upstream creation date
                if (In.ContainsKey(Constants.DefaultStreamName) && In[Constants.DefaultStreamName].Source != null)
                    return In[Constants.DefaultStreamName].Source.CacheLastRefresh;

                // if no relevant up-stream, just return now!
                return DateTime.Now;
            }
        }

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// The app this data-source is attached to
        /// </summary>
        public virtual int AppId { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// The Zone this data-source is attached to
        /// </summary>
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
        protected internal bool _configurationIsLoaded;


        // protected internal Dictionary<string, IValueProvider> Settings 
        /// <summary>
        /// Make sure that configuration-parameters have been parsed (tokens resolved)
        /// but do it only once (for performance reasons)
        /// </summary>
		protected internal virtual void EnsureConfigurationIsLoaded()
        {
            if (_configurationIsLoaded)
                return;

            // Ensure that we have a configuration-provider (not always the case, but required)
            if (ConfigurationProvider == null)
                throw new Exception("No ConfigurationProvider configured on this data-source. Cannot EnsureConfigurationIsLoaded");

            // construct a property access for in, use it in the config provider
            var instancePAs = new Dictionary<string, IValueProvider>() { { "In".ToLower(), new DataTargetValueProvider(this) } };
            ConfigurationProvider.LoadConfiguration(Configuration, instancePAs);
            _configurationIsLoaded = true;
        }

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
        {
            Attach(streamName, dataSource[Constants.DefaultStreamName]);
        }

        public void Attach(string streamName, IDataStream dataStream)
        {
            if (In.ContainsKey(streamName))
                In.Remove(streamName);

            In.Add(streamName, dataStream);
        }

        #region User Interface - not implemented yet
        //public virtual bool AllowUserEdit
        //{
        //    get { return true; }
        //}

        //public virtual bool AllowUserSort
        //{
        //    get { return true; }
        //}

        //public virtual bool AllowVersioningUI
        //{
        //    get { return false; }
        //}
        #endregion

        #region Configuration - not implemented yet
        //public virtual bool IsConfigurable
        //{
        //    get { return false; }
        //}
        #endregion

        #region Internals (Ready)

        /// <summary>
        /// Indicates whether the DataSource is ready for use (initialized/configured)
        /// </summary>
        public virtual bool Ready => (In[Constants.DefaultStreamName].Source != null && In[Constants.DefaultStreamName].Source.Ready);

        public bool TempUsesDynamicOut { get; protected set; } = false;

        #endregion


    }
}