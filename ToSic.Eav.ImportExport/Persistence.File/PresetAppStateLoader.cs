using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Logging;
using ToSic.Eav.Repositories;
using ToSic.Eav.Types;

namespace ToSic.Eav.Persistence.File
{
    // Experimental #PresetInAppState
    public class PresetAppStateLoader: HasLog<IPresetLoader>, IPresetLoader
    {
        //private readonly IGlobalTypes _globalTypes;
        private readonly GlobalTypeLoader _globalTypeLoader;

        #region Constructor
        public PresetAppStateLoader(LogHistory logHistory, /*IGlobalTypes globalTypes,*/ GlobalTypeLoader globalTypeLoader) : base($"{LogNames.Eav}.FasLdr")
        {
            logHistory.Add(GlobalTypes.LogHistoryGlobalTypes, Log);
            //_globalTypes = globalTypes;
            _globalTypeLoader = globalTypeLoader;
        }

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
                    var dbTypes = _globalTypeLoader.LoadTypes().ToList(); // _globalTypes.AllContentTypes().Select(p => p.Value).ToList();
                    app.InitContentTypes(dbTypes);
                    typeTimer.Stop();
                    Log.Add($"timers types:{typeTimer.Elapsed}");
                }
                else
                    Log.Add("skipping content-type load");

                // load data
                if (startAt <= AppStateLoadSequence.ItemLoad)
                {
                    //LoadEntities(app, entityIds);
                }
                else
                    Log.Add("skipping items load");

                //Log.Add($"timers sql:sqlAll:{_sqlTotalTime}");
                wrapLog("ok");
            });

            return outerWrapLog("ok", app);
        }
    }
}
