using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Delegate to get Entities when needed
	/// </summary>
	public delegate IDictionary<int, IEntity> GetDictionaryDelegate();

    /// <summary>
    /// The light list for the series of items returned
    /// </summary>
    /// <returns></returns>
    public delegate IEnumerable<IEntity> GetIEnumerableDelegate(); 

	/// <summary>
	/// A DataStream to get Entities when needed
	/// </summary>
	public class DataStream : IDataStream
	{
		private readonly GetDictionaryDelegate _dictionaryDelegate;
	    private readonly GetIEnumerableDelegate _lightListDelegate;


        #region Self-Caching and Results-Persistence Properties / Features
        /// <summary>
        /// This one will return the original result if queried again - as long as this object exists
        /// </summary>
        public bool KeepResultsAfterFirstQuery { get; set; }

        /// <summary>
        /// Place the stream in the cache if wanted, by default not
        /// </summary>
        public bool AutoCaching { get; set; }

        /// <summary>
        /// Default cache duration is 3600
        /// </summary>
        public int CacheDurationInSeconds { get; set; }


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
		/// <param name="dictionaryDelegate">Function which gets Entities</param>
		public DataStream(IDataSource source, string name, GetDictionaryDelegate dictionaryDelegate, GetIEnumerableDelegate lightListDelegate = null, bool enableAutoCaching = false)
		{
			Source = source;
			Name = name;
			_dictionaryDelegate = dictionaryDelegate;
		    _lightListDelegate = lightListDelegate;
		    AutoCaching = enableAutoCaching;
            
            // Default properties for caching config
            KeepResultsAfterFirstQuery = true;
		    CacheDurationInSeconds = 3600 * 24; // one day, since by default if it caches, it would check upstream for cache-reload
            CacheRefreshOnSourceRefresh = true;
		}

        #region Get Dictionary and Get List
        private IDictionary<int, IEntity> _dicList; 
		public IDictionary<int, IEntity> List
		{
			get
			{
                // already retrieved? then return last result to be faster
                if (_dicList != null && KeepResultsAfterFirstQuery)
                    return _dicList;

			    // new version to build upon the simple list, if a simple list was provided instead Tag:PureEntitiesList
			    if (_dictionaryDelegate == null && _lightListDelegate != null)
			        return _dicList = LightList.ToDictionary(e => e.EntityId, e => e);

			    try
			    {
			        var getList = new GetDictionaryDelegate(_dictionaryDelegate);
			        _dicList = getList();

			        return _dicList;
			    }
			    catch (InvalidOperationException ex)
			    {
			        // this is a special exeption - for example when using SQL. Pass it on to enable proper testing
			        throw;
			    }
				catch (Exception ex)
				{
					throw new Exception($"Error getting List of Stream.\nStream Name: {Name}\nDataSource Name: {Source.Name}", ex);
				}
			}
		}

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
                if (_lightListDelegate == null && _dictionaryDelegate != null)
                    _lightList = List.Select(x => x.Value);
                else
                {
                    try
                    {
                        var getEntitiesDelegate = new GetIEnumerableDelegate(_lightListDelegate);
                        _lightList = getEntitiesDelegate();
                    }
                    catch (InvalidOperationException ex) // this is a special exeption - for example when using SQL. Pass it on to enable proper testing
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


        # region commented out 2015-06-19 as I don't see that it's in use anywhere
        ///// <summary>
        ///// Get Entities based on a list of Ids
        ///// </summary>
        ///// <param name="entityIds">Array of EntityIds</param>
        //public IDictionary<int, IEntity> GetEntities(int[] entityIds)
        //{
        //    if (!Source.Ready)
        //        throw new Exception("Data Source Not Ready");

        //    var originals = List;
        //    return entityIds.Distinct().Where(originals.ContainsKey).ToDictionary(id => id, id => originals[id]);
        //}
        #endregion

        /// <summary>
        /// The source which holds this stream
        /// </summary>
		public IDataSource Source { get; set; }

        /// <summary>
        /// Name - usually the name within the Out-dictionary of the source. For identification and for use in caching-IDs and similar
        /// </summary>
		public string Name { get; }



    }
}
