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

        public Runtime(IServiceProvider sp) : base("Eav.RunTme")
        {
            _serviceProvider = sp;
            // Only add the first time it's really used
            if (LoadLog == null) LoadLog = Log;
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


        public AppState LoadFullAppState()
        {

            var outerWrapLog = Log.Call<AppState>();

            var appState = new AppState(new ParentAppState(null, false, false), Constants.PresetIdentity, Constants.PresetName, Log);

            appState.Load(() =>
            {
                var msg = $"get app data package for a#{appState.AppId}";
                var wrapLog = Log.Call(message: msg, useTimer: true);

                // Prepare metadata lists & relationships etc.
                appState.InitMetadata(new Dictionary<int, string>().ToImmutableDictionary(a => a.Key, a => a.Value));
                appState.Name = Constants.PresetName;
                appState.Folder = Constants.PresetName;

                // prepare content-types
                var wrapLoadTypes = Log.Call(useTimer: true);
                // Just attach all global content-types to this app, as they belong here
                var types = LoadGlobalContentTypes(FsDataConstants.GlobalContentTypeMin);
                appState.InitContentTypes(types);
                wrapLoadTypes($"types loaded");

                // load data
                try
                {
                    // IMPORTANT for whoever may need to debug preloaded data
                    // The Entities have one specialty: Their Metadata is _not_ loaded the normal way (from the AppState)
                    // But it's directly-attached-metadata
                    // That's because it's loaded from the JSON, where the metadata is part of the json-file.
                    // This should probably not cause any problems, but it's important to know
                    // We may optimize / change this some day
                    Log.Add("Update Loaders to know about preloaded Content-Types - otherwise some features will not work");
                    var appTypes = appState.ContentTypes.ToList();
                    Loaders.ForEach(l => l.ResetSerializer(appTypes));

                    Log.Add("Load items");
                    foreach (var entityItemFolder in FsDataConstants.EntityItemFolders)
                    {
                        Log.Add($"Load {entityItemFolder} items");
                        var configs = LoadGlobalItems(entityItemFolder)?.ToList() ?? new List<IEntity>();
                        foreach (var c in configs) appState.Add(c as Entity, null, true);
                    }
                }
                catch (Exception ex)
                {
                    Log.Add("Error: Failed adding Entities");
                    Log.Exception(ex);
                }

                wrapLog("ok");
            });

            return outerWrapLog("ok", appState);
        }

        /// <summary>
        /// Reload App Configuration Items from the File System
        /// </summary>
        public void ReloadConfigEntities()
        {
            var mainWrap = Log.Call();
            var appStates = _serviceProvider.Build<IAppStates>();
            var appState = appStates.GetPresetApp();

            var previousConfig = appState.List.FirstOrDefaultOfType(FeatureConstants.TypeName);
            var prevId = previousConfig?.EntityId;

            appState.Load(() =>
            {
                var wrapLog = Log.Call(message: "Inside loader");
                try
                {
                    Log.Add("Load config items");
                    var configs = LoadGlobalItems(FsDataConstants.ConfigFolder)?.ToList() ?? new List<IEntity>();
                    Log.Add($"Found {configs.Count} items");
                    var featuresOnly = configs.OfType(FeatureConstants.TypeName).ToList();
                    Log.Add($"Found {featuresOnly.Count} items which are {FeatureConstants.TypeName} - expected: 1");
                    Log.Add("Ids of previous and new should match, otherwise we may run into problems. " +
                            $"Prev: {prevId}, New: {featuresOnly.FirstOrDefault()?.EntityId}");
                    foreach (var c in featuresOnly) appState.Add(c as Entity, null, true);
                }
                catch (Exception ex)
                {
                    Log.Add("Error updating config");
                    Log.Exception(ex);
                }

                wrapLog("done");
            });

            mainWrap("ok");
        }
    }
}