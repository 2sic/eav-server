using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// This provides access to the in-memory state, to
    /// - retrieve App catalog information from the cache or system (zones)
    /// - to retrieve app states (or automatically load them if accessed the first time)
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("the name of this class is not final, may change")]
    public class State
    {
        /// <summary>
        /// Run start-up, to set the cache object which itself needs the service provider internally.
        /// So this is to ensure DI is preserved
        /// </summary>
        internal static void StartUp(IAppsCache appsCache)
        {
            Cache = appsCache;
        }

        // New implementation 2021-08-28 2dm / experimental
        [PrivateApi] public static IAppsCache Cache { get; private set; }

        //[PrivateApi]
        //public static IAppsCache Cache
        //{
        //    get
        //    {
        //        if (_appsCacheSingleton != null) return _appsCacheSingleton;

        //        var appsCache = Factory.GetServiceProvider().Build<IAppsCache>();
        //        if (appsCache.EnforceSingleton)
        //            _appsCacheSingleton = appsCache;
        //        return appsCache;
        //    }
        //}
        //private static IAppsCache _appsCacheSingleton;


        [InternalApi_DoNotUse_MayChangeWithoutNotice]
        public static AppState Get(int appId) => Cache.Get(appId);

        [InternalApi_DoNotUse_MayChangeWithoutNotice]
        public static AppState Get(IAppIdentity app) => Cache.Get(app);

        [InternalApi_DoNotUse_MayChangeWithoutNotice]
        public static IAppIdentity Identity(int? zoneId, int? appId) => Cache.GetIdentity(zoneId, appId);

        [InternalApi_DoNotUse_MayChangeWithoutNotice] 
        public static IReadOnlyDictionary<int, Zone> Zones => Cache.Zones;

    }
}
