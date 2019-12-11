using System;
using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps.Caching
{
    [PrivateApi("WIP")]
    public interface IZoneCache 
    {
        #region Cache Purging

        ///// <summary>
        ///// Clean cache for specific Zone and App
        ///// </summary>
        //void PurgeCache(int zoneId, int appId);

		/// <summary>
		/// Clean global cache (currently contains List of Zones and Apps)
		/// </summary>
		void PurgeGlobalCache();

        //void PartialUpdate(IEnumerable<int> entities);

        #endregion


        /// <summary>
        /// Get/Resolve ZoneId and AppId for specified ZoneId and/or AppId. If both are null, default ZoneId with it's default App is returned.
        /// </summary>
        /// <returns>Item1 = ZoneId, Item2 = AppId</returns>
        [PrivateApi("todo rename")]
		Tuple<int, int> GetZoneAppId(int? zoneId = null, int? appId = null);


    }
}
