using System;
using System.Text.RegularExpressions;
using ToSic.Eav.Logging;
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

        private readonly DbDataController _db;
        private readonly AppManager _appManager;
        private readonly IRepositoryLoader _repositoryLoader;
        private int _zoneId;

        public AppCreator(DbDataController db, AppManager appManager, IRepositoryLoader repositoryLoader) : base("Eav.AppBld")
        {
            _db = db;
            _appManager = appManager;
            _repositoryLoader = repositoryLoader.Init(Log);
        }

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
        public void Create(string appName)
        {
            // check if invalid app-name
            if (appName == Constants.ContentAppName || appName == Constants.DefaultAppName || string.IsNullOrEmpty(appName) || !Regex.IsMatch(appName, "^[0-9A-Za-z -_]+$"))
                throw new ArgumentOutOfRangeException("appName '" + appName + "' not allowed");

            var appId = CreateInDb();

            // must get app from DB directly, not from cache, so no State.Get(...)
            var appState = _repositoryLoader.AppState(appId, false);

            new AppInitializer(_appManager.ServiceProvider)
                .Init(appState, Log)
                .InitializeApp(appName);
        }

        private int CreateInDb()
        {
            Log.Add("create new app");
            var appGuid = Guid.NewGuid().ToString();
            var app = _db.Init(_zoneId, null, Log).App.AddApp(null, appGuid);

            SystemManager.PurgeZoneList(Log);
            Log.Add($"app created a:{app.AppId}, guid:{appGuid}");
            return app.AppId;
        }

    }
}
