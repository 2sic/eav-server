using System;
using System.Collections;
using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// The light list for the series of items returned
    /// </summary>
    /// <returns></returns>
    public delegate IEnumerable<IEntity> GetIEnumerableDelegate(); 

	/// <inheritdoc />
	/// <summary>
	/// A DataStream to get Entities when needed
	/// </summary>
	public class DataStream : IDataStream
	{
	    private readonly GetIEnumerableDelegate _lightListDelegate;


        #region Self-Caching and Results-Persistence Properties / Features
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
	    /// <param name="lightListDelegate">Function which gets Entities</param>
	    /// <param name="enableAutoCaching"></param>
	    public DataStream(IDataSource source, string name, GetIEnumerableDelegate lightListDelegate = null, bool enableAutoCaching = false)
		{
			Source = source;
			Name = name;
		    _lightListDelegate = lightListDelegate;
		    AutoCaching = enableAutoCaching;
            
            // Default properties for caching config
            ReuseInitialResults = true;
		    CacheDurationInSeconds = 3600 * 24; // one day, since by default if it caches, it would check upstream for cache-reload
            CacheRefreshOnSourceRefresh = true;
		}

        #region Get Dictionary and Get List

	    private IEnumerable<IEntity> _list; 
        public IEnumerable<IEntity> List
	    {
            get
            {
                // already retrieved? then return last result to be faster
                if (_list != null && ReuseInitialResults)
                    return _list;

                #region Check if it's in the cache - and if yes, if it's still valid and should be re-used --> return if found
                if (AutoCaching)
			    {
                    // todo 2rm: this is where we would add a Mutex-style cache-is-building for SharePoint and other slow loaders
                    // similar to the BaseCache.cs lines 89 - 100
                    // but that code should be in the ListCache, so here it's probably just a
                    // probably something like:
			        // var itemInCache = Source.Cache.Lists.Get(this, waitIfBuilding = true);

                    var itemInCache = Source.Cache.Lists.Get(this);
			        var isInCache = itemInCache != null;

			        var refreshCache = !isInCache || CacheRefreshOnSourceRefresh && Source.CacheTimestamp > itemInCache.SourceRefresh;
			        var useCache = isInCache && !refreshCache; 

			        if(useCache)
			            return _list = itemInCache.LightList;

                    // todo 2rm, then mark that we're building or something like
                    // Source.Cache.Lists.SetBuildingMark(this);
                    // and then below, ensure that where necessary, it's released on all errors
                    // maybe by wrapping everything in another try-catch with a Source.Cache.Lists.ReleaseBuildingMark(this)
                }
                #endregion

                #region Assemble the list - either from the DictionaryDelegate or from the LightListDelegate
                // try to use the built-in Entities-Delegate, but if not defined, use other delegate; just make sure we test both, to prevent infinite loops
                if(_lightListDelegate == null)
                    throw new Exception("can't load stream - no delegate found to supply it");
                try
                {
                    var getEntitiesDelegate = new GetIEnumerableDelegate(_lightListDelegate);
                    _list = getEntitiesDelegate();
                }
                catch (InvalidOperationException) // this is a special exeption - for example when using SQL. Pass it on to enable proper testing
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        $"Error getting List of Stream.\nStream Name: {Name}\nDataSource Name: {Source.Name}", ex);
                }

                #endregion

                #region place in cache if so needed

                if (AutoCaching && ReuseInitialResults)
                    // second criteria important to prevent infinite loops
                    Source.Cache.Lists.Set(this, CacheDurationInSeconds);

                #endregion

                return _list;

            }
	    }
        #endregion

	    public void PurgeList(bool cascade = false)
	    {
	        Source.Cache.Lists.Remove(this);
	        if (cascade) Source.PurgeList(cascade);
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
