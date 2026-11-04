using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Apps.Sys.State.AppStateBuilder;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Repositories.Sys;
using ToSic.Sys.Utils.Assemblies;

namespace ToSic.Eav.Apps.Sys.AppStateInFolder;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class AppStateInFolderLoader : ServiceBase, IAppStateLoader
{
    #region Constructor and DI

    public AppStateInFolderLoader(IServiceProvider sp, Generator<FileSystemLoader, FileSystemLoaderOptions> fslGenerator, Generator<IAppStateBuilder> stateBuilder)
        : base("Eav.RunTme", connect: [/* sp (never) */ fslGenerator, stateBuilder])
    {
        _serviceProvider = sp;
        _fslGenerator = fslGenerator;
        _stateBuilder = stateBuilder;

        // Mention constructor in logs
        BootLog.Log.Fn("App Loader constructor", timer: true).Done();

        // Add to global location so the insights can pick this ip.
        // Only add the first time it's really used
        AppStateInFolderGlobalLog.LoadLog ??= Log;
    }

    private readonly IServiceProvider _serviceProvider;
    private readonly Generator<IAppStateBuilder> _stateBuilder;
    private readonly Generator<FileSystemLoader, FileSystemLoaderOptions> _fslGenerator;

    #endregion

    public RepositoryTypes Source => RepositoryTypes.Folder;

    // 1 - find the current path to the .data folder
    [field: AllowNull, MaybeNull]
    public List<string> Paths => field ??= GeneratePaths();

    private List<string> GeneratePaths()
    {
        var l = Log.Fn<List<string>>(message: "start building path-list");

        var all = new List<string>();
        // find all RepositoryInfoOfFolder and let them tell us what paths to use
        var folderRepositoryTypes = AssemblyHandling
            .FindInherited(typeof(FolderBasedRepository), Log)
            .ToListOpt();
        l.A($"found {folderRepositoryTypes.Count} Path providers");

        foreach (var fldRepoType in folderRepositoryTypes)
            try
            {
                l.A($"adding {fldRepoType.FullName}");
                var instance = (FolderBasedRepository)ActivatorUtilities.CreateInstance(_serviceProvider, fldRepoType, []);
                var paths = instance.RootPaths;
                if (paths != null! /* paranoid, as external implementations may not be null-safe */)
                    all.AddRange(paths);
            }
            catch (Exception e)
            {
                l.A($"ran into a problem with one of the path providers: {fldRepoType?.FullName} - will skip.");
                l.Ex(e);
            }
        l.A(l.Try(() => string.Join(",", all)));
        return l.Return(all, $"{all.Count} paths");
    }

    private ToSic.Sys.Logging.LogSettings LogSettings { get; set; } = new();

    internal ICollection<FileSystemLoader> BuildLoadersForGlobal(ToSic.Sys.Logging.LogSettings logSettings)
        => Paths
            .Select((path, loaderIndex) => _fslGenerator.New(new()
                {
                    JsonReaderOptions = FileSystemLoaderOptions.CreateJsonOptionsDyn(KnownAppsConstants.PresetAppId, logSettings, Log),
                    //AppId = KnownAppsConstants.PresetAppId,
                    Path = path,
                    RepoType = Source,
                    IgnoreMissing = true,
                    LogSettings = logSettings,

                    // Set TypeID seed for loader so each loaded type has a unique ID
                    EntityIdSeed = GlobalAppIdConstants.GlobalEntityIdMin +
                                   GlobalAppIdConstants.GlobalEntitySourceSkip * (loaderIndex + 1),

                    // Set TypeID seed for loader so each loaded type has a unique ID
                    TypeIdSeed = GlobalAppIdConstants.GlobalContentTypeMin +
                                 GlobalAppIdConstants.GlobalContentTypeSourceSkip * (loaderIndex + 1),

                })
            )
            .ToListOpt();


    public IAppStateBuilder LoadFullAppState(ToSic.Sys.Logging.LogSettings logSettings)
    {
        LogSettings = logSettings;

        // Get BootLog to make sure it's part of that too
        var bl = BootLog.Log.Fn("Load Full AppState", timer: true);

        // Do normal logging
        var lMain = Log.Fn<IAppStateBuilder>(timer: true);
        lMain.A($"🪵 Using LogSettings: {logSettings}");

        var builder = _stateBuilder.New().InitForPreset();

        var logDetails = LogSettings is { Enabled: true, Details: true };
        ICollection<FileSystemLoader> entityLoaders;
        builder.Load("get app data package", appState =>
        {
            var l = Log.Fn("load app data package");

            // Prepare metadata lists & relationships etc.
            builder.InitMetadata();
            builder.SetNameAndFolder(KnownAppsConstants.PresetName, KnownAppsConstants.PresetName);

            {
                var l2 = Log.Fn(timer: true);
                var loaders = BuildLoadersForGlobal(LogSettings);
                var (nextLoaders, contentTypes, entities) = LoadGlobalContentTypes(loaders, appState);
                // Just attach all global content-types to this app, as they belong here
                builder.InitContentTypes(contentTypes);

                // Note: all must be Entity, but this setup just ensures the compiler sees it that way
                foreach (var entity in entities.OfType<Entity>())
                    builder.Add(entity, null, logDetails);
                entityLoaders = nextLoaders;
                l2.Done("types loaded");
            }

            // load data
            try
            {
                // IMPORTANT for whoever may need to debug preloaded data
                // The Entities have one specialty: Their Metadata is _not_ loaded the normal way (from the AppState)
                // But it's directly-attached-metadata
                // That's because it's loaded from the JSON, where the metadata is part of the json-file.
                // This should probably not cause any problems, but it's important to know
                // We may optimize / change this some day
                // Update 2024-09-01 2dm: made some changes, ATM _some_ of it already uses a shared source for relationships, but
                // only on content-type-sub-entities, and a separate source for the others...?
                l.A("Update Loaders to know about preloaded Content-Types - otherwise some features will not work");

                // 2026-11-04 2dm: this is a rewrite of the previous non-functional-style code,
                // but after analysis it appears that all code paths ended up resetting the loader again
                // so it appears that it had no impact; will comment out for now.
                // Old code was:
                //var appTypes = builder.Reader.ContentTypes.ToListOpt();
                //foreach (var ldr in Loaders)
                //    ldr.ResetSerializer(appTypes);
                // Rewrite was:
                //var newLoaders = loaders
                //    .Select(ldr =>
                //        _fslGenerator.New(ldr.GetLatestOptions() with
                //        {
                //            PrefetchedContentTypesSource = appTypes
                //        }))
                //    .ToList();


                // 2026-11-04 2dm: special case if app reader was specified
                // so types are known and can be retrieved from the AppReader,
                // and all other settings should not be relevant
                var newLoaders = entityLoaders
                    .Select(ldr =>
                        _fslGenerator.New(ldr.GetLatestOptions() with
                        {
                            JsonReaderOptions = new(builder.Reader),
                        }))
                    .ToList();

                l.A("Load items");

                var entities = LoadGlobalEntities(newLoaders, builder.Reader.AppId);
                l.A($"Load entity {entities.Count} items");
                
                // Note: all must be Entity, but this setup just ensures the compiler sees it that way
                foreach (var entity in entities.OfType<Entity>()) 
                    builder.Add(entity, null, logDetails);
            }
            catch (Exception ex)
            {
                l.A("Error: Failed adding Entities");
                l.Ex(ex);
            }

            l.Done();
        });

        bl.Done();
        return lMain.ReturnAsOk(builder);
    }

}