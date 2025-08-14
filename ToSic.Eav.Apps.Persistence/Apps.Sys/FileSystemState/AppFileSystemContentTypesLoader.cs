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
    public IList<IContentType> ContentTypes(IEntitiesSource entitiesSource)
    {
        var l = Log.Fn<IList<IContentType>>();
        try
        {
            var extPaths = GetAllExtensionDataPaths();
            l.A($"Found {extPaths.Count} extensions with {FolderConstants.DataFolderProtected} folder");
            var allTypes = extPaths
                .SelectMany(p => LoadTypesFromOneExtensionPath(p, entitiesSource))
                .Distinct(new EqualityComparer_ContentType())
                .ToListOpt();
            return l.Return(allTypes, "ok");
        }
        catch (Exception e)
        {
            l.A("error " + e.Message);
            return l.Return([], "error");
        }
    }


    private IEnumerable<IContentType> LoadTypesFromOneExtensionPath(string extensionPath, IEntitiesSource entitiesSource)
    {
        var l = Log.IfSummary(LogSettings).Fn<IEnumerable<IContentType>>(extensionPath);
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
        return l.Return(types, $"found: {types.Count}");
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