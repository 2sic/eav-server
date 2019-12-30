using ToSic.Eav.Caching;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// This helps retrieve App catalog information from the cache or system.
    /// </summary>
    [PrivateApi]
    public class Apps
    {

        [PrivateApi]
        public static IAppsCache Cache
        {
            get
            {
                if (_appsCacheSingleton != null) return _appsCacheSingleton;

                var appsCache = Factory.Resolve<IAppsCache>();
                if (appsCache.EnforceSingleton)
                    _appsCacheSingleton = appsCache;
                return appsCache;
            }
        }
        private static IAppsCache _appsCacheSingleton;


        [PrivateApi]
        public static AppState Get(int appId) => Cache.Get(appId);

        [PrivateApi]
        public static AppState Get(IAppIdentity app) => Cache.Get(app);

        [PrivateApi]
        public static IAppIdentity Identity(int? zoneId, int? appId) => Cache.GetIdentity(zoneId, appId);

    }
}
