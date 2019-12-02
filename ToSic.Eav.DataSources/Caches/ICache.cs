using System;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Interfaces;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.DataSources.Caches
{
    /// <summary>
    /// Caching interface for Root Eav Cache. 
    /// </summary>
    [VisualQuery(GlobalName = "ToSic.Eav.DataSources.Caches.ICache, ToSic.Eav.DataSources",
        Type = DataSourceType.Source)]
    public interface ICache : IDataSource
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
		IContentType GetContentType(string name);

		/// <summary>
		/// Gets a ContentType by Id
		/// </summary>
		IContentType GetContentType(int contentTypeId);

		/// <summary>
		/// Get/Resolve ZoneId and AppId for specified ZoneId and/or AppId. If both are null, default ZoneId with it's default App is returned.
		/// </summary>
		/// <returns>Item1 = ZoneId, Item2 = AppId</returns>
		Tuple<int, int> GetZoneAppId(int? zoneId = null, int? appId = null);

        IListsCache Lists { get; }

        AppState AppState { get; }

        void PreLoadCache(string primaryLanguage);

    }
}
