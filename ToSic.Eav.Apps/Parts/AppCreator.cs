using System;
using System.Text.RegularExpressions;
using ToSic.Lib.Logging;
using ToSic.Eav.Repositories;
using ToSic.Eav.Repository.Efc;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Special tool just to create an app.
    /// It's not part of the normal AppManager / ZoneManager, because when it's initialized it doesn't yet have a real app identity
    /// </summary>
    public class AppCreator: ServiceBase
    {
        #region Constructor / DI

        private int _zoneId;

        public AppCreator(DbDataController db, IRepositoryLoader repositoryLoader, SystemManager systemManager, Generator<AppInitializer> appInitGenerator) : base("Eav.AppBld")
        {
            ConnectServices(
                _db = db,
                _appInitGenerator = appInitGenerator,
                SystemManager = systemManager,
                RepositoryLoader = repositoryLoader
            );
        }
        private readonly DbDataController _db;
        private readonly Generator<AppInitializer> _appInitGenerator;
        protected readonly SystemManager SystemManager;
        protected readonly IRepositoryLoader RepositoryLoader;

        public AppCreator Init(int zoneId)
        {
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

            _appInitGenerator.New()
                .Init(appState)
                .InitializeApp(appName);
        }

        private int CreateInDb(string appGuid, int? inheritAppId)
        {
            Log.A("create new app");
            var app = _db.Init(_zoneId, null).App.AddApp(null, appGuid, inheritAppId);

            SystemManager.PurgeZoneList();
            Log.A($"app created a:{app.AppId}, guid:{appGuid}");
            return app.AppId;
        }

    }
}
