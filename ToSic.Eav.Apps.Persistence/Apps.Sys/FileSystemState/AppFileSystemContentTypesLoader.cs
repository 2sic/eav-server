using ToSic.Eav.Apps.Integration;
using ToSic.Eav.Apps.Sys.PresetLoaders;
using ToSic.Eav.Context;
using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Integration;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Apps.Sys.FileSystemState;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppFileSystemContentTypesLoader(ISite siteDraft, Generator<FileSystemLoader> fslGenerator, LazySvc<IAppPathsMicroSvc> appPathsLazy, LazySvc<IZoneMapper> zoneMapper)
    : AppFileSystemLoaderBase(siteDraft, appPathsLazy, zoneMapper, connect: [fslGenerator]), IAppContentTypesLoader
{
    public new IAppContentTypesLoader Init(IAppReader app, LogSettings logSettings)
        => base.Init(app, logSettings) as IAppContentTypesLoader;

    /// <inheritdoc />
    public IList<IContentType> ContentTypes(IEntitiesSource entitiesSource)
    {
        var l = Log.Fn<IList<IContentType>>();
        try
        {
            var extPaths = GetAllExtensionPaths();
            l.A($"Found {extPaths.Count} extensions with .data folder");
            var allTypes = extPaths
                .SelectMany(p => LoadTypesFromOneExtensionPath(p, entitiesSource))
                .Distinct(new EqualityComparer_ContentType())
                .ToList();
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
        var fsLoader = fslGenerator.New().Init(AppIdentity.AppId, extensionPath, RepositoryTypes.Folder, true, entitiesSource, LogSettings);
        var types = fsLoader.ContentTypes();
        return l.Return(types, $"found: {types.Count}");
    }



    #region Helpers

    private List<string> GetAllExtensionPaths()
    {
        var l = Log.IfSummary(LogSettings).Fn<List<string>>();
        var dir = new DirectoryInfo(Path);
        if (!dir.Exists) return l.Return([], $"directory do not exist: {dir}");
        var sub = dir.GetDirectories();
        var subDirs = sub
            .SelectMany(s => s
                .GetDirectories(FolderConstants.AppDataProtectedFolder)
                .Where(d => d.Exists)
                .SelectMany(a => a.GetDirectories(FolderConstants.FolderSystem))
                .Union(s.GetDirectories(FolderConstants.FolderOldDotData))
            );
        var paths = subDirs
            .Where(d => d.Exists)
            .Select(s => s.FullName)
            .ToList();
        return l.Return(paths, $"OK, paths:{string.Join(";", paths)}");
    }

    #endregion
}