using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Caching;

namespace ToSic.Eav.Apps
{
    public class AppStates: IAppStates
    {

        public AppStates(IAppsCache appsCache) => Cache = appsCache;
        private readonly IAppsCache Cache;

        /// <inheritdoc />
        public AppState Get(IAppIdentity app) => Cache.Get(app);

        /// <inheritdoc />
        public AppState Get(int appId) => Cache.Get(appId);

        public IAppIdentity Identity(int? zoneId, int? appId) => Cache.GetIdentity(zoneId, appId);
    }
}
