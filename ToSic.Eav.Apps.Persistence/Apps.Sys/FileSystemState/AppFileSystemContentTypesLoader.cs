using ToSic.Eav.Apps.Sys.Paths;
using ToSic.Eav.Apps.Sys.PresetLoaders;
using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys.ZoneMapper;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Apps.Sys.FileSystemState;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppFileSystemContentTypesLoader(ISite siteDraft, Generator<FileSystemLoader, FileSystemLoaderOptions> fslGenerator, LazySvc<IAppPathsMicroSvc> appPathsLazy, LazySvc<IZoneMapper> zoneMapper)
    : AppFileSystemLoaderBase(siteDraft, appPathsLazy, zoneMapper, connect: [fslGenerator]), IAppContentTypesLoader
{
    /// <inheritdoc />
    public PartialData TypesAndEntities(IEntitiesSource entitiesSource)
    {
        var l = Log.Fn<PartialData>();
        try
        {
            var extPaths = GetAllExtensionDataPaths();
            l.A($"Found {extPaths.Count} extensions with {FolderConstants.DataFolderProtected} folder");
            
            var allData = extPaths
                .Select((p, index) => LoadDataFromOneExtensionPath(p, -(index + 1) * 1_000, entitiesSource))
                .ToListOpt();

            var allTypes = allData
                .SelectMany(pair => pair.ContentTypes)
                // Not sure why this is necessary, best document when we ever find out
                .Distinct(new EqualityComparer_ContentType())
                .ToListOpt();

            var allEntities = allData
                .SelectMany(pair => pair.Entities)
                //.Distinct()
                .ToListOpt();

            return l.Return(new(allTypes, allEntities), $"types: {allTypes.Count}; entities: {allEntities.Count}");
        }
        catch (Exception e)
        {
            l.A("error " + e.Message);
            return l.ReturnAsError(new([], []));
        }
    }


    private (ICollection<IContentType> ContentTypes, ICollection<IEntity> Entities) LoadDataFromOneExtensionPath(
        string extensionPath, int seed, IEntitiesSource entitiesSource)
    {
        var l = Log.IfSummary(LogSettings).Fn<(ICollection<IContentType>, ICollection<IEntity>)>(extensionPath);
        var fsLoader = fslGenerator.New(options: new()
        {
            AppId = AppIdentity.AppId,
            Path = extensionPath,
            RepoType = RepositoryTypes.Folder,
            IgnoreMissing = true,
            EntitiesSource = entitiesSource,
            LogSettings = LogSettings,
        });
        // Expect local entities to be very few...
        fsLoader.EntityIdSeed = seed;
        fsLoader.EntityIdDirection = -1;
        fsLoader.TypeIdSeed = seed;
        fsLoader.TypeIdDirection = -1;

        var types = fsLoader.ContentTypes();
        // WIP - ATM hard-wired to bundles, but should probably go through all instead...
        var entities = fsLoader.Entities(AppDataFoldersConstants.BundlesFolder, entitiesSource);
        return l.Return((types, entities), $"types: {types.Count}; entities: {entities.Count}");
    }



    #region Helpers

    private ICollection<string> GetAllExtensionDataPaths()
    {
        var l = Log.IfSummary(LogSettings).Fn<ICollection<string>>(
            $"Roots: {ExtensionsPath};{ExtensionsPathShared}");

        var roots = new[]
        {
            ExtensionsPath,              // local
            ExtensionsPathShared,        // shared
        };

        var paths = roots
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .SelectMany(GetDataPathsFromRoot)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToListOpt();

        return l.Return(paths, $"OK, paths:{string.Join(";", paths)}");
    }

    private static IEnumerable<string> GetDataPathsFromRoot(string rootPath)
    {
        var root = new DirectoryInfo(rootPath);
        if (!root.Exists) return [];

        // For each extension in the root, find App_Data/system
        return root
            .GetDirectories()
            .SelectMany(ext =>
                ext.GetDirectories(FolderConstants.DataFolderProtected)
                   .Where(d => d.Exists)
                   .SelectMany(ad => ad.GetDirectories(FolderConstants.DataSubFolderSystem)))
            .Where(d => d.Exists)
            .Select(d => d.FullName);
    }

    #endregion
}