﻿using System;
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
	public class DataStream : IDataStream
	{
	    private readonly GetIEnumerableDelegate _listDelegate;


        #region Self-Caching and Results-Persistence Properties / Features

        [PrivateApi] public virtual string CacheSuffix => "";

        /// <inheritdoc />
        /// <summary>
        /// This one will return the original result if queried again - as long as this object exists
        /// </summary>
        public bool ReuseInitialResults { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Place the stream in the cache if wanted, by default not
        /// </summary>
        public bool AutoCaching { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Default cache duration is 3600
        /// </summary>
        public int CacheDurationInSeconds { get; set; }


        /// <inheritdoc />
        /// <summary>
        /// Kill the cache if the source data is newer than the cache-stamped data
        /// </summary>
        public bool CacheRefreshOnSourceRefresh { get; set; }

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
            ReuseInitialResults = true;
		    CacheDurationInSeconds = 3600 * 24; // one day, since by default if it caches, it would check upstream for cache-reload
            CacheRefreshOnSourceRefresh = true;
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
                var wrapLog = Source.Log.Call<IImmutableList<IEntity>>($"{nameof(Name)}:{Name}; {nameof(ReuseInitialResults)}:{ReuseInitialResults}");
                // already retrieved? then return last result to be faster
                if (_list != null && ReuseInitialResults)
                    return wrapLog("reuse", _list);

                IImmutableList<IEntity> EntityListDelegate()
                {
                    #region Assemble the list - either from the DictionaryDelegate or from the LightListDelegate
                    // try to use the built-in Entities-Delegate, but if not defined, use other delegate; just make sure we test both, to prevent infinite loops
                    if (_listDelegate == null)
                        throw new Exception("can't load stream - no delegate found to supply it");
                    try
                    {
                        var getEntitiesDelegate = new GetIEnumerableDelegate(_listDelegate);
                        return getEntitiesDelegate().ToImmutableList();
                    }
                    catch (InvalidOperationException) // this is a special exception - for example when using SQL. Pass it on to enable proper testing
                    {
                        wrapLog("error", null);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        var msg = $"Error getting List of Stream.\nStream Name: {Name}\nDataSource Name: {Source.Name}";
                        // Source.Log.Add(msg);
                        wrapLog("error", null);
                        throw new Exception(msg, ex);
                    }

                    #endregion
                }

                #region Check if it's in the cache - and if yes, if it's still valid and should be re-used --> return if found
                if (AutoCaching && ReuseInitialResults)
                {
                    Source.Log.Add($"{nameof(AutoCaching)} && {nameof(ReuseInitialResults)}");
                    var cacheItem = new ListCache(Source.Log).GetOrBuild(this, EntityListDelegate, CacheDurationInSeconds);
                    return _list = wrapLog("ok", cacheItem.List);
                }
                #endregion

                var result = EntityListDelegate();
                if (ReuseInitialResults)
                    _list = result;
                return wrapLog("ok", result);
            }
	    }
        #endregion

	    public void PurgeList(bool cascade = false)
	    {
            new ListCache(Source.Log).Remove(this);
	        if (cascade) Source.PurgeList(true);
	    }

	    [Obsolete("deprecated since 2sxc 9.8 / eav 4.5 - use List instead")]
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


        #region Experimental support for IEnumerable<IEntity> - WIP for https://github.com/2sic/2sxc/issues/1700
        // if this works well, we would then put it in the IDataStream interface

        public IEnumerator<IEntity> GetEnumerator() => List.GetEnumerator();

	    IEnumerator IEnumerable.GetEnumerator() => List.GetEnumerator();
        #endregion Experimental support for IEnumerable<IEntity>
    }
}
