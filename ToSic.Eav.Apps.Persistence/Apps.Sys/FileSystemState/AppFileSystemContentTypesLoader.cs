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
    public (ICollection<IContentType> ContentTypes, ICollection<IEntity> Entities) TypesAndEntities(IEntitiesSource entitiesSource)
    {
        ILogCall<(ICollection<IContentType> ContentTypes, ICollection<IEntity> Entities)>? l = Log.Fn<(ICollection<IContentType> ContentType, ICollection<IEntity> Entities)>();
        try
        {
            var extPaths = GetAllExtensionDataPaths();
            l.A($"Found {extPaths.Count} extensions with {FolderConstants.DataFolderProtected} folder");
            
            var allData = extPaths
                .Select(p => LoadDataFromOneExtensionPath(p, entitiesSource))
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

            return l.Return((allTypes, allEntities), $"types: {allTypes.Count}; entities: {allEntities.Count}");
        }
        catch (Exception e)
        {
            l.A("error " + e.Message);
            return l.ReturnAsError(([], []));
        }
    }


    private (ICollection<IContentType> ContentTypes, ICollection<IEntity> Entities) LoadDataFromOneExtensionPath(string extensionPath, IEntitiesSource entitiesSource)
    {
        var l = Log.IfSummary(LogSettings).Fn<(ICollection<IContentType>, ICollection<IEntity>)>(extensionPath);
        var fsLoader = fslGenerator.New(options: new()
        {
            AppId = AppIdentity.AppId,
            Path = extensionPath,
            RepoType = RepositoryTypes.Folder,
            IgnoreMissing =true,
            EntitiesSource= entitiesSource,
            LogSettings= LogSettings,
        });
        var types = fsLoader.ContentTypes();
        // WIP - ATM hard-wired to bundles, but should probably go through all instead...
        var entities = fsLoader.Entities(AppDataFoldersConstants.BundlesFolder, entitiesSource);
        return l.Return((types, entities), $"types: {types.Count}; entities: {entities.Count}");
    }



    #region Helpers

    private ICollection<string> GetAllExtensionDataPaths()
    {
        var l = Log.IfSummary(LogSettings).Fn<ICollection<string>>($"Path: {Path}");
        var dir = new DirectoryInfo(Path);
        if (!dir.Exists)
            return l.Return([], $"directory do not exist: {dir}");
        var sub = dir.GetDirectories();
        var subDirs = sub
            .SelectMany(s => s
                .GetDirectories(FolderConstants.DataFolderProtected)
                .Where(d => d.Exists)
                .SelectMany(a => a.GetDirectories(FolderConstants.DataSubFolderSystem))
                // disabled in v20
                //.Union(s.GetDirectories(FolderConstants.FolderOldDotData))
            );
        var paths = subDirs
            .Where(d => d.Exists)
            .Select(s => s.FullName)
            .ToListOpt();
        return l.Return(paths, $"OK, paths:{string.Join(";", paths)}");
    }

    #endregion
}