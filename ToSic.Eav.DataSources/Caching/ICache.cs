using System;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Caches;
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
    public interface IRootCache : IDataSource
    {
		/// <summary>
		/// Clean cache for specific Zone and App
		/// </summary>
		void PurgeCache(int zoneId, int appId);

		/// <summary>
		/// Clean global cache (currently contains List of Zones and Apps)
		/// </summary>
		void PurgeGlobalCache();

		/// <summary>
		/// Gets a ContentType by Name
		/// </summary>
		[PrivateApi]
		IContentType GetContentType(string name);

		/// <summary>
		/// Gets a ContentType by Id
		/// </summary>
		[PrivateApi]
		IContentType GetContentType(int contentTypeId);

		/// <summary>
		/// Get/Resolve ZoneId and AppId for specified ZoneId and/or AppId. If both are null, default ZoneId with it's default App is returned.
		/// </summary>
		/// <returns>Item1 = ZoneId, Item2 = AppId</returns>
		[PrivateApi]
		Tuple<int, int> GetZoneAppId(int? zoneId = null, int? appId = null);

        /// <summary>
        /// Advanced caching of lists which have queried some of the data in this cache, and want to retain the results of the query till the root changes. 
        /// </summary>
        IListsCache Lists { get; }

        /// <summary>
        /// Retrieve the AppState of the current app.
        /// </summary>
        AppState AppState { get; }

        [PrivateApi]
        void PreLoadCache(string primaryLanguage);

    }
}
