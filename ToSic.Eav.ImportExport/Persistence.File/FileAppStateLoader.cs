using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Logging;
using ToSic.Eav.Types;

namespace ToSic.Eav.Persistence.File
{
    // Experimental #PresetInAppState
    public class FileAppStateLoaderWIP: HasLog<FileAppStateLoaderWIP>
    {
        private readonly IAppStates _appStates;
        private readonly IGlobalTypes _globalTypes;
        #region Constructor
        public FileAppStateLoaderWIP(IAppStates appStates, LogHistory logHistory, IGlobalTypes globalTypes) : base($"{LogNames.Eav}.FasLdr")
        {
            logHistory.Add(GlobalTypes.LogHistoryGlobalTypes, Log);
            _appStates = appStates;
            _globalTypes = globalTypes;
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
            var appIdentity = new AppIdentity(Constants.PresetZoneId, Constants.PresetAppId); //  _appStates.Identity(null, appId);
            var appGuidName = Constants.PresetName; // _appStates.AppIdentifier(appIdentity.ZoneId, appIdentity.AppId);
            var appState = Update(new AppState(_appStates, new ParentAppState(null, false), appIdentity, appGuidName, Log), AppStateLoadSequence.Start);

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
                    //var nameAndFolder = PreLoadAppPath(app.AppId);
                    app.Name = Constants.PresetName; // nameAndFolder?.Item1;
                    app.Folder = Constants.PresetName; // nameAndFolder?.Item2;
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
                    var dbTypes = _globalTypes.AllContentTypes().Select(p => p.Value).ToList();
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
