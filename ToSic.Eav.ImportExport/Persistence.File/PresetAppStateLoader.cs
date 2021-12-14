using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Repositories;
using ToSic.Eav.Run;

namespace ToSic.Eav.Persistence.File
{
    // Experimental #PresetInAppState
    public class PresetAppStateLoader: HasLog<IPresetLoader>, IPresetLoader
    {
        #region Constructor
        public PresetAppStateLoader(LogHistory logHistory, IRuntime runtimeLoader) : base($"{LogNames.Eav}.FasLdr")
        {
            logHistory.Add(LogNames.LogHistoryGlobalTypes, Log);
            _runtimeLoader = runtimeLoader.Init(Log);
            LoadLog = Log;
        }

        public static ILog LoadLog = null;

        private readonly IRuntime _runtimeLoader;

        #endregion

        public AppState AppState(int appId)
        {
            var wrapLog = Log.Call<AppState>($"{appId}");

            var appState = LoadBasicAppState(appId);

            return wrapLog("loaded", appState);
        }

        private AppState LoadBasicAppState(int appId)
        {
            var wrapLog = Log.Call<AppState>($"AppId: {appId}");
            var appIdentity = new AppIdentity(Constants.PresetZoneId, Constants.PresetAppId);
            var appGuidName = Constants.PresetName;
            var appState = Update(new AppState(new ParentAppState(null, false), appIdentity, appGuidName, Log), AppStateLoadSequence.Start);

            return wrapLog("ok", appState);
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
                    //_sqlTotalTime = _sqlTotalTime.Add(InitMetadataLists(app, _dbContext));
                    // TODO: this might fail, as we don't have a list of Metadata
                    app.InitMetadata(new Dictionary<int, string>().ToImmutableDictionary(a => a.Key, a => a.Value));
                    app.Name = Constants.PresetName;
                    app.Folder = Constants.PresetName;
                }
                else
                    Log.Add("skipping metadata load");

                if (startAt <= AppStateLoadSequence.ContentTypeLoad)
                    startAt = AppStateLoadSequence.ContentTypeLoad;

                // prepare content-types
                if (startAt <= AppStateLoadSequence.ContentTypeLoad)
                {
                    var typeTimer = Stopwatch.StartNew();
                    // Just attach all global content-types to this app, as they belong here
                    var dbTypes = _runtimeLoader.LoadGlobalContentTypes(Global.GlobalContentTypeMin);
                    app.InitContentTypes(dbTypes);
                    typeTimer.Stop();
                    Log.Add($"timers types:{typeTimer.Elapsed}");
                }
                else
                    Log.Add("skipping content-type load");

                // load data
                if (startAt <= AppStateLoadSequence.ItemLoad)
                {
                    //var configs = LoadConfigItems(Global.GroupConfiguration);
                    //var queries = LoadConfigItems(Global.GroupQuery);
                    //const int zeroPaddings = 10000000;
                    //var fakeIdCount = int.MaxValue / zeroPaddings * zeroPaddings;
                    //foreach (var config in configs)
                    //{
                    //    app.Add(config as Entity, null, true);
                    //}
                    //LoadEntities(app, entityIds);
                }
                else
                    Log.Add("skipping items load");

                //Log.Add($"timers sql:sqlAll:{_sqlTotalTime}");
                wrapLog("ok");
            });

            return outerWrapLog("ok", app);
        }

        private List<IEntity> LoadConfigItems(string identifier)
        {
            var wrapLog = Log.Call<List<IEntity>>(identifier);

            try
            {
                var list = _runtimeLoader.LoadGlobalItems(identifier)?.ToList() ?? new List<IEntity>();
                return wrapLog($"{list.Count}", list);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                return wrapLog("error", new List<IEntity>());
            }

        }
    }
}
