using System.IO;
using ToSic.Eav.Context;
using ToSic.Eav.Data.Source;
using ToSic.Eav.Integration;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Apps.Integration;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppFileSystemContentTypesLoader(ISite siteDraft, Generator<FileSystemLoader> fslGenerator, LazySvc<IAppPathsMicroSvc> appPathsLazy, LazySvc<IZoneMapper> zoneMapper)
    : AppFileSystemLoaderBase(siteDraft, appPathsLazy, zoneMapper, connect: [fslGenerator]), IAppContentTypesLoader
{
    public new IAppContentTypesLoader Init(IAppReader app)
        => base.Init(app) as IAppContentTypesLoader;

    /// <inheritdoc />
    public IList<IContentType> ContentTypes(IEntitiesSource entitiesSource)
    {
        var l = Log.Fn<IList<IContentType>>();
        try
        {
            var extPaths = ExtensionPaths();
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
        var l = Log.Fn<IEnumerable<IContentType>>(extensionPath);
        var fsLoader = fslGenerator.New().Init(AppIdentity.AppId, extensionPath, RepositoryTypes.Folder, true, entitiesSource);
        var types = fsLoader.ContentTypes();
        return l.Return(types);
    }



    #region Helpers

    private List<string> ExtensionPaths()
    {
        var l = Log.Fn<List<string>>();
        var dir = new DirectoryInfo(Path);
        if (!dir.Exists) return l.Return([], $"directory do not exist: {dir}");
        var sub = dir.GetDirectories();
        var subDirs = sub
            .SelectMany(s => s
                .GetDirectories(Constants.AppDataProtectedFolder)
                .Where(d => d.Exists)
                .SelectMany(a => a.GetDirectories(Constants.FolderSystem))
                .Union(s.GetDirectories(Constants.FolderOldDotData))
            );
        var paths = subDirs
            .Where(d => d.Exists)
            .Select(s => s.FullName)
            .ToList();
        return l.Return(paths, $"OK, paths:{string.Join(";", paths)}");
    }

    #endregion
}