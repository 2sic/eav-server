﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Streams;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources.Caching
{
    /// <summary>
    /// Special DataSource which automatically caches everything it's given.
    /// It's Used to optimize queries, so that heavier calculations don't need to be repeated if another request with the same signature is used. <br/>
    /// Internally it asks all up-stream DataSources what factors would determine their caching.
    /// So if part of the supplying DataSources would have a changed parameter (like a different filter), it will still run the full query and cache the results again. 
    /// </summary>
    /// <remarks>
    /// * Changed in v15.05 to use the [immutable convention](xref:NetCode.Conventions.Immutable)
    /// * note that the above change is actually a breaking change, but since this is such an advanced DataSource, we assume it's not used in dynamic code.
    /// </remarks>
    [VisualQuery(
        NiceName = "Cache Streams",
        UiHint = "Cache all streams based on some rules",
        Icon = Icons.HistoryOff,
        Type = DataSourceType.Cache, 
        NameId = "ToSic.Eav.DataSources.Caching.CacheAllStreams, ToSic.Eav.DataSources",
        DynamicOut = true,
        DynamicIn = true,
        ConfigurationType = "|Config ToSic.Eav.DataSources.Caches.CacheAllStreams",
        NameIds = new []
            {
                "ToSic.Eav.DataSources.Caches.CacheAllStreams, ToSic.Eav.DataSources"
            },
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-CacheAllStreams")]
    [PublicApi_Stable_ForUseInYourCode]
	public class CacheAllStreams : DataSource
    {
        #region Configuration-properties

		/// <summary>
		/// How long to keep these streams in the cache.
		/// Default is `0` - meaning fall back to 1 day
		/// </summary>
		[Configuration(Fallback = 0)]
        public int CacheDurationInSeconds => Configuration.GetThis(0);

        /// <summary>
        /// If a source-refresh should trigger a cache rebuild
        /// </summary>
        [Configuration(Fallback = true)]
        public bool RefreshOnSourceRefresh => Configuration.GetThis(true);

        /// <summary>
        /// Perform a cache rebuild async. 
        /// </summary>
        [Configuration(Fallback = false)]
        public bool ReturnCacheWhileRefreshing => Configuration.GetThis(false);


		private readonly IDictionary<string, IDataStream> _out = new Dictionary<string, IDataStream>(StringComparer.InvariantCultureIgnoreCase);
		private bool _requiresRebuildOfOut = true;

        /// <inheritdoc />
		public override IDictionary<string, IDataStream> Out
		{
			get
			{
                if (!_requiresRebuildOfOut) return _out;
                // now create all streams
                CreateOutWithAllStreams();
                _requiresRebuildOfOut = false;
                return _out;
			}
		}
		#endregion

		/// <inheritdoc />
		/// <summary>
		/// Constructs a new App DataSource
		/// </summary>
		[PrivateApi]
		public CacheAllStreams(MyServices services): base(services, $"{DataSourceConstants.LogPrefix}.CachAl")
		{
			// this one is unusual, so don't pre-attach a default data stream
            //OutIsDynamic = true;
        }

		/// <summary>
		/// Create a stream for each data-type
		/// </summary>
		private void CreateOutWithAllStreams()
		{
            Configuration.Parse();

            // attach all missing streams, now that Out is used the first time
            // note that some streams were already added because of the DeferredOut
            foreach (var dataStream in In.Where(s => !_out.ContainsKey(s.Key)))
                _out.Add(dataStream.Key, StreamWithCaching(dataStream.Key));
        }

	    private IDataStream StreamWithCaching(string name)
        {
            var outStream = new DataStream(this, name,  () => In[name].List, true);

            // only set if a value other than 0 (= default) was given
            if (CacheDurationInSeconds != 0)
	            outStream.CacheDurationInSeconds = CacheDurationInSeconds;
	        outStream.CacheRefreshOnSourceRefresh = RefreshOnSourceRefresh;
            return outStream;
        }
        
	}

}