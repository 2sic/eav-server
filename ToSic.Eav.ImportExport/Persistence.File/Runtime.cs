using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Apps;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repositories;
using ToSic.Eav.Run;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Persistence.File
{
    public partial class Runtime : ServiceBase, IRuntime
    {
        #region Constructor and DI

        public Runtime(IServiceProvider sp, Generator<FileSystemLoader> fslGenerator) : base("Eav.RunTme")
        {
            _serviceProvider = sp;
            ConnectServices(
                _fslGenerator = fslGenerator
            );
            // Only add the first time it's really used
            if (LoadLog == null) LoadLog = Log;
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly Generator<FileSystemLoader> _fslGenerator;
        public static ILog LoadLog = null;


        #endregion

        public RepositoryTypes Source => RepositoryTypes.Folder;

        // 1 - find the current path to the .data folder
        public List<string> Paths
        {
            get
            {
                if (_paths != null) return _paths;
                var wrapLog = Log.Fn<List<string>>(message: "start building path-list");

                _paths = new List<string>();
                // find all RepositoryInfoOfFolder and let them tell us what paths to use
                var types = AssemblyHandling.FindInherited(typeof(FolderBasedRepository), Log).ToList();
                Log.A($"found {types.Count} Path providers");

                foreach (var typ in types)
                    try
                    {
                        Log.A($"adding {typ.FullName}");
                        var instance = (FolderBasedRepository) ActivatorUtilities.CreateInstance(_serviceProvider, typ, Array.Empty<object>());
                        var paths = instance.RootPaths;
                        if (paths != null) _paths.AddRange(paths);
                    }
                    catch(Exception e)
                    {
                        Log.A($"ran into a problem with one of the path providers: {typ?.FullName} - will skip.");
                        Log.Ex(e);
                    }
                Log.A(() => string.Join(",", _paths));
                return wrapLog.Return(_paths, $"{_paths.Count} paths");
            }
        }
        private List<string> _paths;

        




        internal List<FileSystemLoader> Loaders => _loader ?? (_loader = Paths
            .Select(path => _fslGenerator.New().Init(Constants.PresetAppId, path, Source, true, null)).ToList());
        private List<FileSystemLoader> _loader;


        public AppState LoadFullAppState()
        {
            var outerWrapLog = Log.Fn<AppState>(startTimer: true);

            var appState = new AppState(new ParentAppState(null, false, false), Constants.PresetIdentity, Constants.PresetName, Log);

            appState.Load(() =>
            {
                var msg = $"get app data package for a#{appState.AppId}";
                var wrapLog = Log.Fn(message: msg, startTimer: true);

                // Prepare metadata lists & relationships etc.
                appState.InitMetadata(new Dictionary<int, string>().ToImmutableDictionary());
                appState.Name = Constants.PresetName;
                appState.Folder = Constants.PresetName;

                // prepare content-types
                // Experimental new log wrapping...

                //var wrapLoadTypes = Log.Fn(startTimer: true);
                //// Just attach all global content-types to this app, as they belong here
                //var types = LoadGlobalContentTypes(FsDataConstants.GlobalContentTypeMin);
                //appState.InitContentTypes(types);
                //wrapLoadTypes.Done($"types loaded");

                Log.DoAndLog(startTimer: true, action: () =>
                {
                    var types = LoadGlobalContentTypes(FsDataConstants.GlobalContentTypeMin);
                    // Just attach all global content-types to this app, as they belong here
                    appState.InitContentTypes(types);
                    return "types loaded";
                });

                // load data
                try
                {
                    // IMPORTANT for whoever may need to debug preloaded data
                    // The Entities have one specialty: Their Metadata is _not_ loaded the normal way (from the AppState)
                    // But it's directly-attached-metadata
                    // That's because it's loaded from the JSON, where the metadata is part of the json-file.
                    // This should probably not cause any problems, but it's important to know
                    // We may optimize / change this some day
                    Log.A("Update Loaders to know about preloaded Content-Types - otherwise some features will not work");
                    var appTypes = appState.ContentTypes.ToList();
                    Loaders.ForEach(l => l.ResetSerializer(appTypes));

                    Log.A("Load items");

                    var entitySets = LoadAndDeduplicateEntitySets();

                    foreach (var eSet in entitySets)
                    {
                        Log.A($"Load {eSet.Folder} - {eSet.Entities.Count} items");
                        foreach (var c in eSet.Entities) appState.Add(c as Entity, null, true);
                    }
                }
                catch (Exception ex)
                {
                    Log.A("Error: Failed adding Entities");
                    Log.Ex(ex);
                }

                wrapLog.Done("ok");
            });

            return outerWrapLog.ReturnAsOk(appState);
        }

    }
}