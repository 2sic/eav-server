using System;
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
		//private readonly GetDictionaryDelegate _dictionaryDelegate;
	    private readonly GetIEnumerableDelegate _lightListDelegate;


        #region Self-Caching and Results-Persistence Properties / Features
        /// <inheritdoc />
        /// <summary>
        /// This one will return the original result if queried again - as long as this object exists
        /// </summary>
        public bool KeepResultsAfterFirstQuery { get; set; }

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
            KeepResultsAfterFirstQuery = true;
		    CacheDurationInSeconds = 3600 * 24; // one day, since by default if it caches, it would check upstream for cache-reload
            CacheRefreshOnSourceRefresh = true;
		}

        #region Get Dictionary and Get List

	    private IEnumerable<IEntity> _lightList; 
        public IEnumerable<IEntity> LightList
	    {
            get
            {
                // already retrieved? then return last result to be faster
                if (_lightList != null && KeepResultsAfterFirstQuery)
                    return _lightList;

                #region Check if it's in the cache - and if yes, if it's still valid and should be re-used --> return if found
                if (AutoCaching)
			    {
			        var itemInCache = Source.Cache.ListGet(this);
			        var isInCache = itemInCache != null;

			        var refreshCache = !isInCache || (CacheRefreshOnSourceRefresh && Source.CacheLastRefresh > itemInCache.SourceRefresh);
			        var useCache = isInCache && !refreshCache; // || ReturnCacheWhileRefreshing

			        if(useCache)
			            return _lightList = itemInCache.LightList;
                }
                #endregion

                #region Assemble the list - either from the DictionaryDelegate or from the LightListDelegate
                // try to use the built-in Entities-Delegate, but if not defined, use other delegate; just make sure we test both, to prevent infinite loops
                //if (_lightListDelegate == null && _dictionaryDelegate != null)
                //    _lightList = LightList;//.Select(x => x.Value);
                if(_lightListDelegate == null)
                    throw new Exception("can't load stream - no delegate found to supply it");
                else
                {
                    try
                    {
                        var getEntitiesDelegate = new GetIEnumerableDelegate(_lightListDelegate);
                        _lightList = getEntitiesDelegate();
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
                }
                #endregion

                #region place in cache if so needed

                if (AutoCaching && KeepResultsAfterFirstQuery)
                    // second criteria important to prevent infinite loops
                    Source.Cache.ListSet(this, CacheDurationInSeconds);

                #endregion

                return _lightList;

            }
	    }
        #endregion


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



    }
}
