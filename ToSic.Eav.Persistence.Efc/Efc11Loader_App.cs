﻿using System;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repositories;
using ToSic.Eav.Run;
using ToSic.Eav.Serialization;
using ToSic.Eav.Types;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Persistence.Efc
{
    /// <summary>
    /// Will load all DB data into the memory data model using Entity Framework Core 1.1
    /// </summary>
    public partial class Efc11Loader: HasLog, IRepositoryLoader
    {

        #region constructor and private vars

        public Efc11Loader(EavDbContext dbContext, 
            Lazy<IEnvironment> environmentLazy, 
            IServiceProvider serviceProvider,
            IAppInitializedChecker initializedChecker) : base("Db.Efc11")
        {
            ServiceProvider = serviceProvider;
            _dbContext = dbContext;
            _environmentLazy = environmentLazy;
            _initializedChecker = initializedChecker;
        }

        public Efc11Loader UseExistingDb(EavDbContext dbContext)
        {
            _dbContext = dbContext;
            return this;
        }

        private IServiceProvider ServiceProvider { get; }
        private EavDbContext _dbContext;
        private readonly Lazy<IEnvironment> _environmentLazy;
        private readonly IAppInitializedChecker _initializedChecker;

        #endregion


        #region AppPackage

        /// <inheritdoc />
        public AppState AppState(int appId, /*int[] entityIds = null,*/ ILog parentLog = null)
        {
            var appIdentity = State.Identity(null, appId);
            var appGuidName = State.Cache.Zones[appIdentity.ZoneId].Apps[appIdentity.AppId];
            var appState = Update(new AppState(appIdentity, appGuidName, parentLog), AppStateLoadSequence.Start, /*entityIds*/null, parentLog);

            return appState;
        }

        public AppState AppState(int appId, bool ensureInitialized, ILog parentLog = null)
        {
            var appState = AppState(appId, parentLog);
            if (!ensureInitialized) return appState;

            // Note: Ignore ensureInitialized on the content app
            // The reason is that this app - even when empty - is needed in the cache before data is imported
            // So if we initialize it, then things will result in duplicate settings/resources/configuration
            // Note that to ensure the Content app works, we must perform the same check again in the 
            // API Endpoint which will edit this data
            if (appState.AppGuidName == Constants.DefaultAppName) return appState;

            return _initializedChecker.EnsureAppConfiguredAndInformIfRefreshNeeded(appState, null, Log)
                ? AppState(appId, parentLog)
                : appState;
        }

        public AppState Update(AppState app, AppStateLoadSequence startAt, int[] entityIds = null, ILog parentLog = null)
        {
            app.Load(parentLog, () =>
            {
                Log.LinkTo(app.Log);
                Log.Add($"get app data package for a#{app.AppId}, " +
                                                    $"startAt: {startAt}, " +
                                                    $"ids only:{entityIds != null}");
                var wrapLog = Log.Call(useTimer: true);

                // prepare metadata lists & relationships etc.
                if (startAt <= AppStateLoadSequence.MetadataInit)
                {
                    _sqlTotalTime = _sqlTotalTime.Add(InitMetadataLists(app, _dbContext));
                    // New in V11.01
                    var nameAndFolder = PreLoadAppPath(app.AppId);
                    app.Name = nameAndFolder?.Item1;
                    app.Folder = nameAndFolder?.Item2;
                }
                else
                    Log.Add("skipping metadata load");

                if (startAt <= AppStateLoadSequence.ContentTypeLoad /*&& app.ContentTypesShouldBeReloaded*/)
                    startAt = AppStateLoadSequence.ContentTypeLoad;

                // prepare content-types
                if (startAt <= AppStateLoadSequence.ContentTypeLoad)
                {
                    var typeTimer = Stopwatch.StartNew();
                    var dbTypes = ContentTypes(app.AppId, app);
                    dbTypes = LoadExtensionsTypesAndMerge(app, dbTypes);
                    app.InitContentTypes(dbTypes);
                    typeTimer.Stop();
                    Log.Add($"timers types:{typeTimer.Elapsed}");
                }
                else
                    Log.Add("skipping content-type load");

                // load data
                if (startAt <= AppStateLoadSequence.ItemLoad)
                    LoadEntities(app, entityIds);
                else
                    Log.Add("skipping items load");

                Log.Add($"timers sql:sqlAll:{_sqlTotalTime}");
                wrapLog("ok");
            });
            return app;
        }

        /// <summary>
        /// Must load the app-path from the settings early on, so that other loaders have it
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        private Tuple<string, string> PreLoadAppPath(int appId)
        {
            var wrapLog = Log.Call<Tuple<string, string>>();
            var nullTuple = new Tuple<string, string>(null, null);
            try
            {
                var dbEntity = GetRawEntities(new int[0], appId, false, "2SexyContent-App");
                if (!dbEntity.Any()) return wrapLog("not in db", nullTuple);
                var json = dbEntity.FirstOrDefault()?.Json;
                if (string.IsNullOrEmpty(json)) return wrapLog("no json", nullTuple);

                Log.Add("app Entity found - this json: " + json);
                var serializer = ServiceProvider.Build<IDataDeserializer>();
                serializer.Initialize(0, ReflectionTypes.FakeCache.Values, null, Log);
                if (!(serializer.Deserialize(json, true, true) is Entity appEntity))
                    return wrapLog("can't deserialize", nullTuple);
                var path = appEntity.GetBestValue<string>(AppLoadConstants.FieldFolder);
                var name = appEntity.GetBestValue<string>(AppLoadConstants.FieldName);

                return string.IsNullOrWhiteSpace(path) 
                    ? wrapLog("no folder", new Tuple<string, string>(name, path)) 
                    : wrapLog(path, new Tuple<string, string>(name, path));
            }
            catch (Exception ex)
            {
                // Ignore, but log
                Log.Add("error " + ex.Message);
            }

            return wrapLog("error", nullTuple);
        }

        #endregion

    }
}
