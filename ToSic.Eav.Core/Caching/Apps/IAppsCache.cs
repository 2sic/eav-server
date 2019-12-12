using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Caching.Apps
{
    /// <summary>
    /// Marks the objects which are responsible for caching <see cref="AppState"/> in memory. <br/>
    /// This is a very powerful system ensuring performance and more. 
    /// </summary>
    [PublicApi]
    public interface IAppsCache
    {

        #region Get an App

        /// <summary>
        /// Retrieve an app from the cache
        /// </summary>
        /// <param name="app">App identifier.</param>
        /// <returns>The <see cref="AppState"/> of the app.</returns>
        AppState Get(IInAppAndZone app);

        #endregion

        #region Look up IDs

        /// <summary>
        /// Get/Resolve ZoneId and AppId for specified ZoneId and/or AppId. If both are null, default ZoneId with it's default App is returned.
        /// </summary>
        /// <returns>Item1 = ZoneId, Item2 = AppId</returns>
        IInAppAndZone GetIdentity(int? zoneId = null, int? appId = null);

        /// <summary>
        /// The list of zones, which internally contains the list of apps. 
        /// </summary>
        Dictionary<int, Zone> Zones { get; }

        #endregion

        #region inspect cache

        /// <summary>
        /// Check if something is already in the cache
        /// </summary>
        /// <param name="app">App identifier.</param>
        /// <returns></returns>
        bool Has(IInAppAndZone app);

        #endregion

        #region Cache Purging

        /// <summary>
        /// Clean cache for specific Zone and App
        /// </summary>
        void PurgeCache(/*int zoneId, int appId*/IInAppAndZone app);

        /// <summary>
        /// Tell the cache to do a partial update on an app
        /// </summary>
        /// <param name="app">App identifier.</param>
        /// <param name="entities">List of entities which need to be updates.</param>
        /// <param name="log">Log object to log what's happening.</param>
        void PartialUpdate(IInAppAndZone app, IEnumerable<int> entities, ILog log);

        /// <summary>
        /// Clean entire global cache, which includes the List of Zones and Apps as well as all the apps.
        /// </summary>
        void PurgeGlobalCache();

        #endregion

        #region PreLoading of Cache when the primary language needs to be specified

        /// <summary>
        /// Load an app into cache, specifying the primary language.
        /// This is used in scenarios, where the primary language cannot be auto-detected, so it's set explicitly.
        /// </summary>
        /// <param name="app">App identifier.</param>
        /// <param name="primaryLanguage">Primary language, lower case.</param>
        void ForceLoad(IInAppAndZone app, string primaryLanguage);

        #endregion

    }
}
