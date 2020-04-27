using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.DataSources.Caching;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{

	/// <inheritdoc />
	/// <summary>
	/// A DataStream to get Entities when needed
	/// </summary>
	[PrivateApi]
	public class DataStream : IDataStream
	{
	    private readonly GetIEnumerableDelegate _listDelegate;


        #region Self-Caching and Results-Persistence Properties / Features

        // 2020-04-27.01 2dm - disabled this - as of now, it's always true, so we'll probably remove it soon
        ///// <inheritdoc />
        ///// <summary>
        ///// This one will return the original result if queried again - as long as this object exists
        ///// </summary>
        //private bool ReuseInitialResults { get; set; } = true;

        /// <inheritdoc />
        /// <summary>
        /// Place the stream in the cache if wanted, by default not
        /// </summary>
        public bool AutoCaching { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Default cache duration is 1 day = 3600 * 24
        /// </summary>
        public int CacheDurationInSeconds { get; set; } = 3600 * 24; // one day, since by default if it caches, it would check upstream for cache-reload


        /// <inheritdoc />
        /// <summary>
        /// Kill the cache if the source data is newer than the cache-stamped data
        /// </summary>
        public bool CacheRefreshOnSourceRefresh { get; set; } = true;

        /// <summary>
        /// Provide access to the CacheKey - so it could be overridden if necessary without using the stream underneath it
        /// </summary>
        public virtual DataStreamCacheStatus Caching => _cachingInternal ?? (_cachingInternal = new DataStreamCacheStatus(Source, Source, Name));
        private DataStreamCacheStatus _cachingInternal;


        #endregion

        /// <summary>
        /// Constructs a new DataStream
        /// </summary>
        /// <param name="source">The DataSource providing Entities when needed</param>
        /// <param name="name">Name of this Stream</param>
        /// <param name="listDelegate">Function which gets Entities</param>
        /// <param name="enableAutoCaching"></param>
        public DataStream(IDataSource source, string name, GetIEnumerableDelegate listDelegate = null, bool enableAutoCaching = false)
		{
			Source = source;
			Name = name;
		    _listDelegate = listDelegate;
		    AutoCaching = enableAutoCaching;
            
            // Default properties for caching config
            //ReuseInitialResults = true;
		    //CacheDurationInSeconds = 3600 * 24; // one day, since by default if it caches, it would check upstream for cache-reload
            //CacheRefreshOnSourceRefresh = true;
		}

        #region Get Dictionary and Get List

        /// <summary>
        /// A temporary result list - must be a List, because otherwise
        /// there's a high risk of IEnumerable signatures with functions being stored inside
        /// </summary>
	    private IImmutableList<IEntity> _list; 
        public IEnumerable<IEntity> List
	    {
            get
            {
                var wrapLog = Source.Log.Call<IImmutableList<IEntity>>($"{nameof(Name)}:{Name}"); // {nameof(ReuseInitialResults)}:{ReuseInitialResults}");
                // If already retrieved return last result to be faster
                if (_list != null) // && ReuseInitialResults)
                    return wrapLog("reuse previous", _list);

                // Check if it's in the cache - and if yes, if it's still valid and should be re-used --> return if found
                if (AutoCaching) // && ReuseInitialResults)
                {
                    Source.Log.Add($"{nameof(AutoCaching)}:{AutoCaching}"); // && {nameof(ReuseInitialResults)}");
                    var cacheItem = new ListCache(Source.Log).GetOrBuild(this, ReadUnderlyingList, CacheDurationInSeconds);
                    return _list = wrapLog("ok", cacheItem.List);
                }

                var result = ReadUnderlyingList();
                // 2020-04-27.01 2dm - disabled this - as of now, it's always true, so we'll probably remove it soon
                // if (ReuseInitialResults)
                _list = result;
                return wrapLog("ok", result);
            }
	    }


        /// <summary>
        /// Assemble the list - from the initially configured ListDelegate
        /// </summary>
        /// <returns></returns>
        IImmutableList<IEntity> ReadUnderlyingList()
        {
            var wrapLog = Source.Log.Call();
            // try to use the built-in Entities-Delegate, but if not defined, use other delegate; just make sure we test both, to prevent infinite loops
            if (_listDelegate == null)
                throw new Exception(Source.Log.Add("can't load stream - no delegate found to supply it"));

            try
            {
                var resultList = new GetIEnumerableDelegate(_listDelegate)().ToImmutableList();
                wrapLog("ok");
                return resultList;
            }
            catch (InvalidOperationException) // this is a special exception - for example when using SQL. Pass it on to enable proper testing
            {
                wrapLog("error");
                throw;
            }
            catch (Exception ex)
            {
                var msg = $"Error getting List of Stream.\nStream Name: {Name}\nDataSource Name: {Source.Name}";
                wrapLog(msg);
                throw new Exception(msg, ex);
            }
        }
        #endregion



        public void PurgeList(bool cascade = false)
        {
            // kill the very local temp cache
            _list = null;
            // kill in list-cache
            new ListCache(Source.Log).Remove(this);
            // tell upstream to flush as well
	        if (cascade) Source.PurgeList(true);
	    }

        // TODO: 11 - REMOVE
	    [Obsolete("deprecated since 2sxc 9.8 / eav 4.5 - use List instead - leave in interface for now, because it might in the signature of other DLLs")]
        [PrivateApi]
	    public IEnumerable<IEntity> LightList => List;


        /// <inheritdoc />
        /// <summary>
        /// The source which holds this stream
        /// </summary>
		public IDataSource Source { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Name - usually the name within the Out-dictionary of the source. For identification and for use in caching-IDs and similar
        /// </summary>
		public string Name { get; }


        #region Support for IEnumerable<IEntity>

        public IEnumerator<IEntity> GetEnumerator() => List.GetEnumerator();

	    IEnumerator IEnumerable.GetEnumerator() => List.GetEnumerator();
        #endregion Support for IEnumerable<IEntity>
    }
}
