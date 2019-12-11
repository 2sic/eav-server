using System;
using System.Collections.Generic;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Caching
{
    [PrivateApi("WIP")]
    public interface IAppsCache
    {

        #region Get an App

        AppState Get(int zoneId, int appId);

        #endregion

        #region Look up IDs

        /// <summary>
        /// Get/Resolve ZoneId and AppId for specified ZoneId and/or AppId. If both are null, default ZoneId with it's default App is returned.
        /// </summary>
        /// <returns>Item1 = ZoneId, Item2 = AppId</returns>
        [PrivateApi("todo rename")]
        IInAppAndZone GetIdentity(int? zoneId = null, int? appId = null);

        Dictionary<int, Zone> ZoneApps { get; }

        /// <summary>
        /// Check if something is already in the cache
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        bool Has(int zoneId, int appId);
        #endregion

        #region Cache Purging

        /// <summary>
        /// Clean cache for specific Zone and App
        /// </summary>
        void PurgeCache(int zoneId, int appId);


        void PartialUpdate(IInAppAndZone app, IEnumerable<int> entities, ILog log);

        /// <summary>
        /// Clean global cache (currently contains List of Zones and Apps)
        /// </summary>
        void PurgeGlobalCache();

        #endregion

        #region PreLoading of Cache (unsure what this is for...)

        /// <summary>
        /// Load an app into cache, specifying the primary language.
        /// This is used in scenarios, where the primary language cannot be auto-detected, so it's set explicitly.
        /// </summary>
        /// <param name="zoneId">Zone ID</param>
        /// <param name="appId">App ID</param>
        /// <param name="primaryLanguage">primary language, lower case</param>
        [PrivateApi]
        void ForceLoad(int zoneId, int appId, string primaryLanguage);

        #endregion

    }
}
