﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Internal.Loaders;
using ToSic.Lib.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repositories;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Persistence.File;

internal partial class AppLoader : ServiceBase, IAppLoader
{
    #region Constructor and DI

    public AppLoader(IServiceProvider sp, Generator<FileSystemLoader> fslGenerator, Generator<IAppStateBuilder> stateBuilder) : base("Eav.RunTme")
    {
        _serviceProvider = sp;
        ConnectServices(
            _fslGenerator = fslGenerator,
            _stateBuilder = stateBuilder
        );
        // Only add the first time it's really used
        InternalAppLoader.LoadLog ??= Log;
    }

    private readonly IServiceProvider _serviceProvider;
    private readonly Generator<IAppStateBuilder> _stateBuilder;
    private readonly Generator<FileSystemLoader> _fslGenerator;

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
            Log.A(Log.Try(() => string.Join(",", _paths)));
            return wrapLog.Return(_paths, $"{_paths.Count} paths");
        }
    }
    private List<string> _paths;


    internal List<FileSystemLoader> Loaders => _loader ??= Paths
        .Select(path => _fslGenerator.New().Init(Constants.PresetAppId, path, Source, true, null)).ToList();
    private List<FileSystemLoader> _loader;


    public IAppStateBuilder LoadFullAppState()
    {
        var outerWrapLog = Log.Fn<IAppStateBuilder>(timer: true);

        var builder = _stateBuilder.New().InitForPreset();
        //var appState = builder.AppState;// new AppState(new ParentAppState(null, false, false), Constants.PresetIdentity, Constants.PresetName, Log);
        // var msg = $"get app data package for a#{appState.AppId}";

        builder.Load("get app data package", appState =>
        {
            var l = Log.Fn("load app data package");
            // Prepare metadata lists & relationships etc.
            builder.InitMetadata();
            builder.SetNameAndFolder(Constants.PresetName, Constants.PresetName);

            l.Do(timer: true, action: () =>
            {
                var types = LoadGlobalContentTypes(appState);
                // Just attach all global content-types to this app, as they belong here
                builder.InitContentTypes(types);
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
                l.A("Update Loaders to know about preloaded Content-Types - otherwise some features will not work");
                var appTypes = builder.Reader.ContentTypes.ToList();
                Loaders.ForEach(ldr => ldr.ResetSerializer(appTypes));

                l.A("Load items");

                var entities = LoadGlobalEntities(builder.Reader);
                l.A($"Load entity {entities.Count} items");
                foreach (var entity in entities) 
                    builder.Add(entity as Entity, null, true);
            }
            catch (Exception ex)
            {
                l.A("Error: Failed adding Entities");
                l.Ex(ex);
            }
        });

        return outerWrapLog.ReturnAsOk(builder);
    }

}