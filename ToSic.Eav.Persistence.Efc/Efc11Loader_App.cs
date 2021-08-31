using System;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Serialization;
using ToSic.Eav.Types;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Persistence.Efc
{
    /// <summary>
    /// Will load all DB data into the memory data model.
    /// It uses Entity Framework Core 1.1 which we use for .net 451 (DNN)
    /// It also works with the last Entity Framework 3, which we use for Oqtane etc.
    /// </summary>
    public partial class Efc11Loader
    {
        #region AppPackage

        /// <summary>
        /// Load the full AppState from the backend - in an un-initialized state (without folder / name etc.).
        /// This is mostly for internal operations where initialization would cause trouble or unexpected side-effects.
        /// </summary>
        /// <param name="appId">AppId (can be different than the appId on current context (e.g. if something is needed from the default appId, like MetaData)</param>
        /// <returns>An object with everything which an app has, usually for caching</returns>
        public AppState LoadBasicAppState(int appId)
        {
            var wrapLog = Log.Call<AppState>();
            var appIdentity =_appStates.Identity(null, appId);
            var appGuidName = _appStates.AppIdentifier(appIdentity.ZoneId, appIdentity.AppId); // State.Cache.Zones[appIdentity.ZoneId].Apps[appIdentity.AppId];
            var appState = Update(new AppState(_appStates, appIdentity, appGuidName, Log), AppStateLoadSequence.Start);

            return wrapLog("ok", appState);
        }

        /// <inheritdoc />
        public AppState AppState(int appId, bool ensureInitialized)
        {
            var wrapLog = Log.Call<AppState>($"{appId}, {ensureInitialized}");

            var appState = LoadBasicAppState(appId);
            if (!ensureInitialized) return wrapLog("won't check initialized", appState);

            // Note: Ignore ensureInitialized on the content app
            // The reason is that this app - even when empty - is needed in the cache before data is imported
            // So if we initialize it, then things will result in duplicate settings/resources/configuration
            // Note that to ensure the Content app works, we must perform the same check again in the 
            // API Endpoint which will edit this data
            if (appState.AppGuidName == Constants.DefaultAppName) return wrapLog("default app, don't auto-init", appState);

            var result = _initializedChecker.EnsureAppConfiguredAndInformIfRefreshNeeded(appState, null, Log)
                ? LoadBasicAppState(appId)
                : appState;
            return wrapLog("with init check", result);
        }

        public AppState Update(AppState app, AppStateLoadSequence startAt, int[] entityIds = null)
        {
            var outerWrapLog = Log.Call<AppState>(message: "What happens inside this is logged in the app-state loading log");

            app.Load(() =>
            {
                var msg = $"get app data package for a#{app.AppId}, startAt: {startAt}, ids only:{entityIds != null}";
                var wrapLog = Log.Call(message: msg, useTimer: true);

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

                if (startAt <= AppStateLoadSequence.ContentTypeLoad)
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

            return outerWrapLog("ok", app);
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
                var path = appEntity.Value<string>(AppLoadConstants.FieldFolder);
                var name = appEntity.Value<string>(AppLoadConstants.FieldName);

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
