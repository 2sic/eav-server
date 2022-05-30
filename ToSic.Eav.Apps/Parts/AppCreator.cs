using System;
using System.Text.RegularExpressions;
using ToSic.Eav.DI;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repositories;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Special tool just to create an app.
    /// It's not part of the normal AppManager / ZoneManager, because when it's initialized it doesn't yet have a real app identity
    /// </summary>
    public class AppCreator: HasLog
    {
        #region Constructor / DI

        private int _zoneId;

        public AppCreator(DbDataController db, IRepositoryLoader repositoryLoader, SystemManager systemManager, Generator<AppInitializer> appInitGenerator) : base("Eav.AppBld")
        {
            _db = db;
            _appInitGenerator = appInitGenerator;
            SystemManager = systemManager.Init(Log);
            RepositoryLoader = repositoryLoader.Init(Log);
        }
        private readonly DbDataController _db;
        private readonly Generator<AppInitializer> _appInitGenerator;
        protected readonly SystemManager SystemManager;
        protected readonly IRepositoryLoader RepositoryLoader;

        public AppCreator Init(int zoneId, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            _zoneId = zoneId;
            return this;
        }

        #endregion

        /// <summary>
        /// Will create a new app in the system and initialize the basic settings incl. the 
        /// app-definition
        /// </summary>
        /// <returns></returns>
        public void Create(string appName, string appGuid = null, int? inheritAppId = null)
        {
            // check if invalid app-name which should never be created like this
            if (appName == Constants.ContentAppName || appName == Constants.DefaultAppGuid || string.IsNullOrEmpty(appName) || !Regex.IsMatch(appName, "^[0-9A-Za-z -_]+$"))
                throw new ArgumentOutOfRangeException("appName '" + appName + "' not allowed");

            var appId = CreateInDb(appGuid ?? Guid.NewGuid().ToString(), inheritAppId);

            // must get app from DB directly, not from cache, so no State.Get(...)
            var appState = RepositoryLoader.AppState(appId, false);

            _appInitGenerator.New // _appManager.ServiceProvider.Build<AppInitializer>()
                .Init(appState, Log)
                .InitializeApp(appName);
        }

        private int CreateInDb(string appGuid, int? inheritAppId)
        {
            Log.A("create new app");
            var app = _db.Init(_zoneId, null, Log).App.AddApp(null, appGuid, inheritAppId);

            SystemManager.PurgeZoneList();
            Log.A($"app created a:{app.AppId}, guid:{appGuid}");
            return app.AppId;
        }

    }
}
