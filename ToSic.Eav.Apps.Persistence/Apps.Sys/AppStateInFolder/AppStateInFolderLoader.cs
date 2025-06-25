using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Apps.Sys.State.AppStateBuilder;

using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Repositories.Sys;
using ToSic.Sys.Utils.Assemblies;

namespace ToSic.Eav.Apps.Sys.AppStateInFolder;

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
        var types = AssemblyHandling
            .FindInherited(typeof(FolderBasedRepository), Log)
            .ToListOpt();
        l.A($"found {types.Count} Path providers");

        foreach (var typ in types)
            try
            {
                l.A($"adding {typ.FullName}");
                var instance = (FolderBasedRepository)ActivatorUtilities.CreateInstance(_serviceProvider, typ, []);
                var paths = instance.RootPaths;
                if (paths != null)
                    all.AddRange(paths);
            }
            catch (Exception e)
            {
                l.A($"ran into a problem with one of the path providers: {typ?.FullName} - will skip.");
                l.Ex(e);
            }
        l.A(l.Try(() => string.Join(",", all)));
        return l.Return(all, $"{all.Count} paths");
    }

    [field: AllowNull, MaybeNull]
    public ICollection<FileSystemLoader> Loaders => field ??= BuildLoaders(null, LogSettings);

    private LogSettings LogSettings { get; set; } = new();

    private ICollection<FileSystemLoader> BuildLoaders(IEntitiesSource? entitiesSource, LogSettings logSettings)
        => Paths
            .Select(path => _fslGenerator
                .New(new()
                {
                    AppId = KnownAppsConstants.PresetAppId,
                    Path = path,
                    RepoType = Source,
                    IgnoreMissing = true,
                    EntitiesSource = entitiesSource,
                    LogSettings = logSettings,
                })
            )
            .ToListOpt();


    public IAppStateBuilder LoadFullAppState(LogSettings logSettings)
    {
        LogSettings = logSettings;

        // Get BootLog to make sure it's part of that too
        var bl = BootLog.Log.Fn("Load Full AppState", timer: true);

        // Do normal logging
        var lMain = Log.Fn<IAppStateBuilder>(timer: true);
        lMain.A($"🪵 Using LogSettings: {logSettings}");

        var builder = _stateBuilder.New().InitForPreset();

        var logDetails = LogSettings is { Enabled: true, Details: true };
        builder.Load("get app data package", appState =>
        {
            var l = Log.Fn("load app data package");
            // Prepare metadata lists & relationships etc.
            builder.InitMetadata();
            builder.SetNameAndFolder(KnownAppsConstants.PresetName, KnownAppsConstants.PresetName);

            l.Do(timer: true, action: () =>
            {
                var (contentTypes, entities) = LoadGlobalContentTypes(appState);
                // Just attach all global content-types to this app, as they belong here
                builder.InitContentTypes(contentTypes);

                // Note: all must be Entity, but this setup just ensures the compiler sees it that way
                foreach (var entity in entities.Where(e => e is Entity).Cast<Entity>())
                    builder.Add(entity, null, logDetails);
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
                // Update 2024-09-01 2dm: made some changes, ATM _some_ of it already uses a shared source for relationships, but
                // only on content-type-sub-entities, and a separate source for the others...?
                l.A("Update Loaders to know about preloaded Content-Types - otherwise some features will not work");
                var appTypes = builder.Reader.ContentTypes.ToListOpt();
                foreach (var ldr in Loaders)
                    ldr.ResetSerializer(appTypes);

                l.A("Load items");

                var entities = LoadGlobalEntities(builder.Reader);
                l.A($"Load entity {entities.Count} items");
                
                // Note: all must be Entity, but this setup just ensures the compiler sees it that way
                foreach (var entity in entities.Where(e => e is Entity).Cast<Entity>()) 
                    builder.Add(entity as Entity, null, logDetails);
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