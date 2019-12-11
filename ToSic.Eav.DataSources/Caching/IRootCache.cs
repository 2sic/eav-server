using ToSic.Eav.Apps.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.DataSources.Caching
{
    /// <summary>
    /// Caching interface for Root Eav Cache. 
    /// </summary>
    [VisualQuery(GlobalName = "ToSic.Eav.DataSources.Caching.IRootCache, ToSic.Eav.DataSources",
        PreviousNames = new []
            {
                "ToSic.Eav.DataSources.Caches.ICache, ToSic.Eav.DataSources"
            },
        Type = DataSourceType.Source)]
    [PublicApi]
    public interface IRootCache : IDataSource //, IAppsCache
    {
        #region Cache Purging

  //      /// <summary>
  //      /// Clean cache for specific Zone and App
  //      /// </summary>
  //      void PurgeCache(int zoneId, int appId);

		///// <summary>
		///// Clean global cache (currently contains List of Zones and Apps)
		///// </summary>
		//void PurgeGlobalCache();

  //      void PartialUpdate(IEnumerable<int> entities);

        #endregion

        #region PreLoading of Cache (unsure what this is for...)

        //[PrivateApi]
        //void PreLoadCache(string primaryLanguage);

        #endregion

        #region Content Type Stuff - probably we should remove this from the RootCache

        /// <summary>
        /// Gets a ContentType by Name
        /// </summary>
        [PrivateApi("probably deprecate, as you should only use the AppState and actually create an AppState, not get it from the root cache?")]
		IContentType GetContentType(string name);

        ///// <summary>
        ///// Gets a ContentType by Id
        ///// </summary>
        //[PrivateApi]
        //IContentType GetContentType(int contentTypeId);

        #endregion

  //      /// <summary>
  //      /// Get/Resolve ZoneId and AppId for specified ZoneId and/or AppId. If both are null, default ZoneId with it's default App is returned.
  //      /// </summary>
  //      /// <returns>Item1 = ZoneId, Item2 = AppId</returns>
  //      [PrivateApi]
		//Tuple<int, int> GetZoneAppId(int? zoneId = null, int? appId = null);

        /// <summary>
        /// Advanced caching of lists which have queried some of the data in this cache, and want to retain the results of the query till the root changes. 
        /// </summary>
        IListCache Lists { get; }

        /// <summary>
        /// Retrieve the AppState of the current app.
        /// </summary>
        AppState AppState { get; }


    }
}
