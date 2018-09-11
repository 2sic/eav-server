﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.VisualQuery;

namespace ToSic.Eav.DataSources.Caches
{
	/// <inheritdoc cref="BaseDataSource" />
	/// <summary>
	/// Return all Entities from a specific App
	/// </summary>

    [VisualQuery(GlobalName = "ToSic.Eav.DataSources.Caches.CacheAllStreams, ToSic.Eav.DataSources",
        Type = DataSourceType.Cache, 
        DynamicOut = true,
        ExpectsDataOfType = "|Config ToSic.Eav.DataSources.Caches.CacheAllStreams",
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-CacheAllStreams")]
	public class CacheAllStreams : BaseDataSource, IDeferredDataSource
	{

        // Todo: caching parameters
        // Time
        // Reload in BG
	    public override string LogId => "DS.CachAl";

        #region Configuration-properties
        private const string RefreshOnSourceRefreshKey = "RefreshOnSourceRefresh";
        private const string CacheDurationInSecondsKey = "CacheDurationInSeconds";
	    private const string ReturnCacheWhileRefreshingKey = "ReturnCacheWhileRefreshing";

		/// <summary>
		/// An alternate app to switch to
		/// </summary>
        public int CacheDurationInSeconds
		{
            get => int.Parse(Configuration[CacheDurationInSecondsKey]);
		    set => Configuration[CacheDurationInSecondsKey] = value.ToString();
		}

        public bool RefreshOnSourceRefresh
	    {
            get => Convert.ToBoolean(Configuration[RefreshOnSourceRefreshKey]);
            set => Configuration[RefreshOnSourceRefreshKey] = value.ToString();
        }

        public bool ReturnCacheWhileRefreshing 
        {
            get => Convert.ToBoolean(Configuration[ReturnCacheWhileRefreshingKey]);
            set => Configuration[ReturnCacheWhileRefreshingKey] = value.ToString();
        }


		private readonly IDictionary<string, IDataStream> _Out = new Dictionary<string, IDataStream>(StringComparer.OrdinalIgnoreCase);
		private bool _requiresRebuildOfOut = true;
		public override IDictionary<string, IDataStream> Out
		{
			get
			{
				if (_requiresRebuildOfOut)
				{
					// now create all streams
					CreateOutWithAllStreams();
					_requiresRebuildOfOut = false;
				}
				return _Out;
			}
		}
		#endregion

		/// <inheritdoc />
		/// <summary>
		/// Constructs a new App DataSource
		/// </summary>
		public CacheAllStreams()
		{
			// this one is unusual, so don't pre-attach a default data stream

			// Set default switch-keys to 0 = no switch
            Configuration.Add(RefreshOnSourceRefreshKey, "[Settings:" + RefreshOnSourceRefreshKey + "||True]");
			Configuration.Add(CacheDurationInSecondsKey, "[Settings:" + CacheDurationInSecondsKey + "||0]"); // 0 is default, meaning don't use custom value, use system value of 1 day
		    Configuration.Add(ReturnCacheWhileRefreshingKey, "False");

            TempUsesDynamicOut = true;
        }

		/// <summary>
		/// Create a stream for each data-type
		/// </summary>
		private void CreateOutWithAllStreams()
		{
            EnsureConfigurationIsLoaded();

            //_Out.Clear();

            // attach all missing streams, now that Out is used the first time
            // note that some streams were already added because of the DeferredOut
		    foreach (var dataStream in In.Where(s => !_Out.ContainsKey(s.Key)))
		    {
		        //var inStream = dataStream.Value as DataStream;
		        AttachDeferredStreamToOut(dataStream.Key);
		    }
		}

        /// <summary>
        /// Will get the stream or if missing, try to attach it first
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
	    private IDataStream GetDeferredStream(string name) 
            => _Out.ContainsKey(name) ? _Out[name] : AttachDeferredStreamToOut(name);

	    private IDataStream AttachDeferredStreamToOut(string name)
        {

            EnsureConfigurationIsLoaded();

	        var outStream = new DataStream(this, name,  () => In[name].List, true);

	        // inStream.AutoCaching = true;
	        if (CacheDurationInSeconds != 0) // only set if a value other than 0 (= default) was given
	            outStream.CacheDurationInSeconds = CacheDurationInSeconds;
	        outStream.CacheRefreshOnSourceRefresh = RefreshOnSourceRefresh;

	        _Out.Add(name, outStream);
	        return outStream;
	    }

        // already attach an out, ready to consume in when it's there
	    public IDataStream DeferredOut(string name) => GetDeferredStream(name);
	}

}