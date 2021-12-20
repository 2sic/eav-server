using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Apps;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repositories;
using ToSic.Eav.Run;

namespace ToSic.Eav.Persistence.File
{
    public partial class Runtime : HasLog<IRuntime>, IRuntime
    {
        #region Constructor and DI

        public Runtime(IServiceProvider sp, LogHistory logHistory) : base("Eav.RunTme")
        {
            _serviceProvider = sp;
            logHistory.Add(LogNames.LogHistoryGlobalTypes, Log);
            LoadLog = Log;
        }

        private readonly IServiceProvider _serviceProvider;
        public static ILog LoadLog = null;


        #endregion

        public RepositoryTypes Source => RepositoryTypes.Folder;

        // 1 - find the current path to the .data folder
        public List<string> Paths
        {
            get
            {
                if (_paths != null) return _paths;
                var wrapLog = Log.Call<List<string>>(message: "start building path-list");

                _paths = new List<string>();
                // find all RepositoryInfoOfFolder and let them tell us what paths to use
                var types = AssemblyHandling.FindInherited(typeof(FolderBasedRepository), Log).ToList();
                Log.Add($"found {types.Count} Path providers");

                foreach (var typ in types)
                    try
                    {
                        Log.Add($"adding {typ.FullName}");
                        var instance = (FolderBasedRepository) ActivatorUtilities.CreateInstance(_serviceProvider, typ, Array.Empty<object>());
                        var paths = instance.RootPaths;
                        if (paths != null) _paths.AddRange(paths);
                    }
                    catch(Exception e)
                    {
                        Log.Add($"ran into a problem with one of the path providers: {typ?.FullName} - will skip.");
                        Log.Exception(e);
                    }
                Log.Add(() => string.Join(",", _paths));
                return wrapLog($"{_paths.Count} paths", _paths);
            }
        }
        private List<string> _paths;

        




        internal List<FileSystemLoader> Loaders => _loader ?? (_loader = Paths
            .Select(path => _serviceProvider.Build<FileSystemLoader>().Init(Constants.PresetAppId, path, Source, true, null, Log)).ToList());
        private List<FileSystemLoader> _loader;


        public AppState AppState()
        {

            var outerWrapLog = Log.Call<AppState>();

            var appState = new AppState(new ParentAppState(null, false), Constants.PresetIdentity, Constants.PresetName, Log);

            appState.Load(() =>
            {
                var msg = $"get app data package for a#{appState.AppId}";
                var wrapLog = Log.Call(message: msg, useTimer: true);

                // Prepare metadata lists & relationships etc.
                // TODO: this might fail, as we don't have a list of Metadata
                appState.InitMetadata(new Dictionary<int, string>().ToImmutableDictionary(a => a.Key, a => a.Value));
                appState.Name = Constants.PresetName;
                appState.Folder = Constants.PresetName;


                // prepare content-types
                var typeTimer = Stopwatch.StartNew();
                // Just attach all global content-types to this app, as they belong here
                var dbTypes = LoadGlobalContentTypes(Global.GlobalContentTypeMin);
                appState.InitContentTypes(dbTypes);
                typeTimer.Stop();
                Log.Add($"timers types:{typeTimer.Elapsed}");

                // load data
                try
                {
                    Log.Add("Update Loaders to have the latest AppState with Content-Types");
                    Loaders.ForEach(l => l.ResetSerializer(appState.ContentTypes.ToList()));

                    Log.Add("Load config items");
                    var configs = LoadGlobalItems(Global.GroupConfiguration)?.ToList() ?? new List<IEntity>();
                    foreach (var c in configs) appState.Add(c as Entity, null, true);

                    Log.Add("Add queries");
                    var queries = LoadGlobalItems(Global.GroupQuery)?.ToList() ?? new List<IEntity>();
                    foreach (var q in queries) appState.Add(q as Entity, null, true);
                }
                catch (Exception ex)
                {
                    Log.Add("Error: Failed adding Entities");
                    Log.Exception(ex);
                }


                //Log.Add($"timers sql:sqlAll:{_sqlTotalTime}");
                wrapLog("ok");
            });

            return outerWrapLog("ok", appState);
        }
    }
}