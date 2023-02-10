using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources.Caching
{
	/// <summary>
	/// Special DataSource which automatically caches everything it's given.
	/// It's Used to optimize queries, so that heavier calculations don't need to be repeated if another request with the same signature is used. <br/>
	/// Internally it asks all up-stream DataSources what factors would determine their caching.
	/// So if part of the supplying DataSources would have a changed parameter (like a different filter), it will still run the full query and cache the results again. 
	/// </summary>

    [VisualQuery(
        NiceName = "Cache Streams",
        UiHint = "Cache all streams based on some rules",
        Icon = Icons.HistoryOff,
        Type = DataSourceType.Cache, 
        GlobalName = "ToSic.Eav.DataSources.Caching.CacheAllStreams, ToSic.Eav.DataSources",
        DynamicOut = true,
        DynamicIn = true,
        ExpectsDataOfType = "|Config ToSic.Eav.DataSources.Caches.CacheAllStreams",
        PreviousNames = new []
            {
                "ToSic.Eav.DataSources.Caches.CacheAllStreams, ToSic.Eav.DataSources"
            },
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-CacheAllStreams")]
    [PublicApi_Stable_ForUseInYourCode]
	public class CacheAllStreams : DataSource
    {
        #region Configuration-properties

		/// <summary>
		/// How long to keep these streams in the cache
		/// </summary>
        public int CacheDurationInSeconds
		{
            get => Configuration.GetThis(0); // int.Parse(Configuration[CacheDurationInSecondsKey]);
            set => Configuration.SetThis(value);
        }

        /// <summary>
        /// If a source-refresh should trigger a cache rebuild
        /// </summary>
        public bool RefreshOnSourceRefresh
        {
            get => Configuration.GetThis(true); // global::System.Convert.ToBoolean(Configuration[RefreshOnSourceRefreshKey]);
            set => Configuration.SetThis(value);
        }

        /// <summary>
        /// Perform a cache rebuild async. 
        /// </summary>
        public bool ReturnCacheWhileRefreshing
        {
            get => Configuration.GetThis(false); // global::System.Convert.ToBoolean(Configuration[ReturnCacheWhileRefreshingKey]);
            set => Configuration.SetThis(value);
        }


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
		public CacheAllStreams(Dependencies dependencies): base(dependencies, $"{DataSourceConstants.LogPrefix}.CachAl")
		{
			// this one is unusual, so don't pre-attach a default data stream
            //OutIsDynamic = true;

			// Set default switch-keys to 0 = no switch
            ConfigMask($"{nameof(RefreshOnSourceRefresh)}||True");
			ConfigMask($"{nameof(CacheDurationInSeconds)}||0"); // 0 is default, meaning don't use custom value, use system value of 1 day
		    ConfigMask($"{nameof(ReturnCacheWhileRefreshing)}||False");
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