using System;
using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Caching
{
    [PrivateApi("WIP")]
    public interface IAppsCache: IHasLog/*, ICacheExpiring*/
    {
        //IAppsCache Init(int zoneId, int appId);

        #region Get an App
        ///// <summary>
        ///// Get the <see cref="AppState"/> of this app from the cache.
        ///// </summary>
        //AppState AppState { get; }

        AppState Get(int zoneId, int appId);

        #endregion

        #region Look up IDs

        /// <summary>
        /// Get/Resolve ZoneId and AppId for specified ZoneId and/or AppId. If both are null, default ZoneId with it's default App is returned.
        /// </summary>
        /// <returns>Item1 = ZoneId, Item2 = AppId</returns>
        [PrivateApi("todo rename")]
        Tuple<int, int> GetZoneAppId(int? zoneId = null, int? appId = null);

        Dictionary<int, Zone> ZoneApps { get; }

        // todo: rename
        bool HasCacheItem(int zoneId, int appId);
        #endregion

        #region Cache Purging

        /// <summary>
        /// Clean cache for specific Zone and App
        /// </summary>
        void PurgeCache(int zoneId, int appId);


        void PartialUpdate(IEnumerable<int> entities);

        /// <summary>
        /// Clean global cache (currently contains List of Zones and Apps)
        /// </summary>
        void PurgeGlobalCache();

        #endregion

        #region PreLoading of Cache (unsure what this is for...)

        [PrivateApi]
        void PreLoadCache(int zoneId, int appId, string primaryLanguage);

        #endregion

        #region Content Type Stuff - probably we should remove this from the RootCache


        #endregion




    }
}
