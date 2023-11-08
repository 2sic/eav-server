using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using ToSic.Eav.Apps;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Serialization;
using System.Runtime.CompilerServices;

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
        private AppState LoadAppStateFromDb(int appId)
        {
            _logStore.Add(EavLogs.LogStoreAppStateLoader, Log);

            var wrapLog = Log.Fn<AppState>($"AppId: {appId}");
            var appIdentity =_appStates.IdentityOfApp(appId);
            var appGuidName = _appStates.AppIdentifier(appIdentity.ZoneId, appIdentity.AppId);

            // This will contain the parent reference - in most cases it's the -42 App
            ParentAppState parent;

            var ancestorAppId = GetAncestorAppIdOrZero(appId);

            if (ancestorAppId != 0)
            {
                // Check if feature is enabled #SharedAppFeatureEnabled
                if (!_featuresService.Value.IsEnabled(BuiltInFeatures.SharedApps))
                    throw new FeaturesDisabledException(BuiltInFeatures.SharedApps.NameId, 
                        $"This is required to load shared app states. " +
                        $"The App {appIdentity.Show()} has an ancestor {ancestorAppId}. " +
                        $"This implies that it has an ancestor. 0 was expected, otherwise you need the feature.");

                var testParentApp = _appStates.Get(ancestorAppId);
                parent = new ParentAppState(testParentApp, true, true);
            }
            else
            {
                // New v13 - use global app by default to share content-types
                var globalApp = _appStates.GetPresetApp();
                if (globalApp == null)
                    throw new Exception("Can't find global app - which is required to build any other apps. ");
                parent = new ParentAppState(globalApp, true, false);
            }


            var appState = Update(new AppState(parent, appIdentity, appGuidName, Log), AppStateLoadSequence.Start);

            return wrapLog.ReturnAsOk(appState);
        }

        private int GetAncestorAppIdOrZero(int appId)
        {
            var wrapLog = Log.Fn<int>($"{nameof(appId)}:{appId}");
            // Prefetch this App (new in v13 for ancestor apps)
            var appInDb = _dbContext.ToSicEavApps.FirstOrDefault(a => a.AppId == appId);
            var appSysSettings = appInDb?.SysSettings;
            if (string.IsNullOrWhiteSpace(appSysSettings))
                return wrapLog.Return(0, "none found");

            var sysSettings = JsonSerializer.Deserialize<AppSysSettings>(appInDb.SysSettings, JsonOptions.SafeJsonForHtmlAttributes);
            if (!sysSettings.Inherit || sysSettings.AncestorAppId == 0) 
                return wrapLog.Return(0, "data found but inherit not active");

            if (sysSettings.AncestorAppId == appId)
            {
                Log.A($"Error: Got an {nameof(sysSettings.AncestorAppId)} of {appId}. " +
                        "It's the same as the app itself - this should never happen. Stop.");
                return wrapLog.Return(0, "error");
            }

            return wrapLog.Return(sysSettings.AncestorAppId, $"found {sysSettings.AncestorAppId}");
        }

            // ReSharper disable ExplicitCallerInfoArgument
        public AppState AppStateRaw(int appId, [CallerFilePath] string cPath = default, [CallerMemberName] string cName = default, [CallerLineNumber] int cLine = default)
            => AppState(appId, false, new CodeRef(cPath, cName, cLine));

        public AppState AppStateInitialized(int appId, [CallerFilePath] string cPath = default, [CallerMemberName] string cName = default, [CallerLineNumber] int cLine = default)
            => AppState(appId, true, new CodeRef(cPath, cName, cLine));
        // ReSharper restore ExplicitCallerInfoArgument

        /// <inheritdoc />
        private AppState AppState(int appId, bool ensureInitialized, CodeRef codeRef)
        {
            var wrapLog = Log.Fn<AppState>($"{appId}, {ensureInitialized}", timer: true);

            var appState = LoadAppStateFromDb(appId);
            if (!ensureInitialized) return wrapLog.Return(appState, "won't check initialized");

            // Note: Ignore ensureInitialized on the content app
            // The reason is that this app - even when empty - is needed in the cache before data is imported
            // So if we initialize it, then things will result in duplicate settings/resources/configuration
            // Note that to ensure the Content app works, we must perform the same check again in the 
            // API Endpoint which will edit this data
            if (appState.NameId == Constants.DefaultAppGuid) return wrapLog.Return(appState, "default app, don't auto-init");

            var result = _initializedChecker.EnsureAppConfiguredAndInformIfRefreshNeeded(appState, null, Log)
                ? LoadAppStateFromDb(appId)
                : appState;
            return wrapLog.Return(result, "with init check");
        }

        public AppState Update(AppState app, AppStateLoadSequence startAt, int[] entityIds = null)
        {
            var lMain = Log.Fn<AppState>(message: "What happens inside this is logged in the app-state loading log");
            
            var msg = $"get app data package for a#{app.AppId}, startAt: {startAt}, ids only:{entityIds != null}";
            app.Load(() => Log.Do(timer: true, message: msg, action: l =>
            {
                // prepare metadata lists & relationships etc.
                if (startAt <= AppStateLoadSequence.MetadataInit)
                {
                    _sqlTotalTime = _sqlTotalTime.Add(InitMetadataLists(app));
                    var nameAndFolder = PreLoadAppPath(app.AppId);
                    app.Name = nameAndFolder.Name;
                    app.Folder = nameAndFolder.Path;
                }
                else
                    l.A("skipping metadata load");

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
                    l.A($"timers types:{typeTimer.Elapsed}");
                }
                else
                    l.A("skipping content-type load");

                // load data
                if (startAt <= AppStateLoadSequence.ItemLoad)
                    LoadEntities(app, entityIds);
                else
                    l.A("skipping items load");

                l.A($"timers sql:sqlAll:{_sqlTotalTime}");
            }));

            return lMain.ReturnAsOk(app);
        }

        /// <summary>
        /// Must load the app-path from the settings early on, so that other loaders have it
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        private (string Name, string Path) PreLoadAppPath(int appId)
        {
            var l = Log.Fn<(string Name, string Path)>(appId.ToString());
            var nullTuple = (null as string, null as string);
            try
            {
                // Get all Entities in the 2SexyContent-App scope
                var dbEntity = GetRawEntities(Array.Empty<int>(), appId, false, AppLoadConstants.TypeAppConfig);
                if (!dbEntity.Any()) return l.Return(nullTuple, "not in db");

                // Get the first one as it should be the one containing the App-Configuration
                // WARNING: This looks a bit fishy, I think it shouldn't just assume the first one is the right one
                var json = dbEntity.FirstOrDefault()?.Json;
                if (string.IsNullOrEmpty(json)) return l.Return(nullTuple, "no json");

                l.A("app Entity found - this json: " + json);
                var serializer = _dataDeserializer.New(); // ServiceProvider.Build<IDataDeserializer>();
                serializer.Initialize(appId, new List<IContentType>(), null);
                if (!(serializer.Deserialize(json, true, true) is Entity appEntity))
                    return l.Return(nullTuple, "can't deserialize");
                var path = appEntity.Value<string>(AppLoadConstants.FieldFolder);
                var name = appEntity.Value<string>(AppLoadConstants.FieldName);

                return string.IsNullOrWhiteSpace(path)
                    ? l.Return((name, path), "no folder")
                    : l.Return((name, path), path);
            }
            catch (Exception ex)
            {
                // Ignore, but log
                l.Ex(ex);
            }

            return l.Return(nullTuple, "error");
        }

        #endregion

    }
}
