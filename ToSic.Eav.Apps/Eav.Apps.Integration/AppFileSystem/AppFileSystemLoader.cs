using System.IO;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Apps.Internal.Work;
using ToSic.Eav.Context;
using ToSic.Eav.Data.Source;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Apps.Integration;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppFileSystemLoader: ServiceBase<AppFileSystemLoader.MyServices>, IAppFileSystemLoader, IAppContentTypesLoader
{
    #region Constants

    public const string FieldFolderPrefix = "field-";
    public const string JsFile = "index.js";

    #endregion

    #region Dependencies and Constructor

    public class MyServices(
        ISite site,
        Generator<FileSystemLoader> fslGenerator,
        LazySvc<IAppPathsMicroSvc> appPathsLazy)
        : MyServicesBase(connect: [site, fslGenerator, appPathsLazy])
    {
        public ISite Site { get; } = site;
        internal Generator<FileSystemLoader> FslGenerator { get; } = fslGenerator;
        internal LazySvc<IAppPathsMicroSvc> AppPathsLazy { get; } = appPathsLazy;
    }

    /// <summary>
    /// DI Constructor
    /// </summary>
    /// <param name="services"></param>
    public AppFileSystemLoader(MyServices services) : this(services, EavLogs.Eav + ".AppFSL") { }

    /// <summary>
    /// Inheritance constructor
    /// </summary>
    /// <param name="services"></param>
    /// <param name="logName"></param>
    protected AppFileSystemLoader(MyServices services, string logName) : base(services, logName)
    {
        Site = services.Site;
    }

    #endregion

    public string Path { get; set; }
    public string PathShared { get; set; }
    protected ISite Site;
    protected IAppIdentity AppIdentity { get; private set; }
    private IAppPaths _appPaths;

    #region Inits

    public IAppFileSystemLoader Init(IAppReader appReader)
    {
        var l = Log.Fn<IAppFileSystemLoader>($"{appReader.AppId}, {appReader.Specs.Folder}, ...");
        AppIdentity = appReader;
        _appPaths = Services.AppPathsLazy.Value.Get(appReader, Site);
        InitPathAfterAppId();
        return l.Return(this);
    }

    IAppContentTypesLoader IAppContentTypesLoader.Init(IAppReader app)
        => Init(app) as IAppContentTypesLoader;

    /// <summary>
    /// Init Path After AppId must be in an own method, as each implementation may have something custom to handle this
    /// </summary>
    /// <returns></returns>
    protected virtual bool InitPathAfterAppId()
    {
        var l = Log.Fn<bool>();
        Path = System.IO.Path.Combine(_appPaths.PhysicalPath, Constants.FolderAppExtensions);
        PathShared = System.IO.Path.Combine(_appPaths.PhysicalPathShared, Constants.FolderAppExtensions);
        return l.ReturnTrue($"p:{Path}, ps:{PathShared}");
    }

    #endregion



    /// <inheritdoc />
    public List<InputTypeInfo> InputTypes()
    {
        var l = Log.Fn<List<InputTypeInfo>>();
        var types = GetInputTypes(Path, AppConstants.AppPathPlaceholder);
        types.AddRange(GetInputTypes(PathShared, AppConstants.AppPathSharedPlaceholder));
        return l.Return(types, $"{types.Count}");
    }

    /// <inheritdoc />
    public IList<IContentType> ContentTypes(IEntitiesSource entitiesSource)
    {
        var l = Log.Fn<IList<IContentType>>();
        try
        {
            var extPaths = ExtensionPaths();
            l.A($"Found {extPaths.Count} extensions with .data folder");
            var allTypes = extPaths.SelectMany(p => LoadTypesFromOneExtensionPath(p, entitiesSource))
                .Distinct(new EqualityComparer_ContentType())
                .ToList();
            return l.Return(allTypes, "ok");
        }
        catch (Exception e)
        {
            l.A("error " + e.Message);
        }

        return l.Return(new List<IContentType>(), "error");
    }


    private IEnumerable<IContentType> LoadTypesFromOneExtensionPath(string extensionPath, IEntitiesSource entitiesSource)
    {
        var l = Log.Fn<IEnumerable<IContentType>>(extensionPath);
        var fsLoader = base.Services.FslGenerator.New().Init(AppIdentity.AppId, extensionPath, RepositoryTypes.Folder, true, entitiesSource);
        var types = fsLoader.ContentTypes();
        return l.Return(types);
    }



    #region Helpers

    private List<InputTypeInfo> GetInputTypes(string path, string placeholder)
    {
        var l = Log.Fn<List<InputTypeInfo>>();
        var di = new DirectoryInfo(path);
        if (!di.Exists) return l.Return([], "directory not found");
        var inputFolders = di.GetDirectories(FieldFolderPrefix + "*");
        Log.A($"found {inputFolders.Length} field-directories");

        var withIndexJs = inputFolders
            .Where(fld => fld.GetFiles(JsFile).Any())
            .Select(fld => fld.Name).ToArray();
        Log.A($"found {withIndexJs.Length} folders with {JsFile}");

        var types = withIndexJs.Select(name =>
            {
                var fullName = name.Substring(FieldFolderPrefix.Length);
                var niceName = NiceName(name);
                // TODO: use metadata information if available
                return new InputTypeInfo(fullName, niceName, "Extension Field", "", false,
                    $"{placeholder}/{Constants.FolderAppExtensions}/{name}/{JsFile}", false, "file-system");
            })
            .ToList();
        return l.Return(types, $"{types.Count}");
    }

    private static string NiceName(string name)
    {
        var nameStack = name.Split('-');
        if (nameStack.Length < 3) return "[Bad Name Format]";
        // drop "field-" and "string-" or whatever type name is used
        nameStack = nameStack.Skip(2).ToArray();
        var caps = nameStack.Select(n =>
        {
            if (string.IsNullOrWhiteSpace(n)) return "";
            if (n.Length <= 1) return n;
            return char.ToUpper(n[0]) + n.Substring(1);
        });

        var niceName = string.Join(" ", caps);
        return niceName;
    }

    private List<string> ExtensionPaths()
    {
        var l = Log.Fn<List<string>>();
        var dir = new DirectoryInfo(Path);
        if (!dir.Exists) return l.Return([], $"directory do not exist: {dir}");
        var sub = dir.GetDirectories();
        var subDirs = sub.SelectMany(
            s => 
                s.GetDirectories(Constants.AppDataProtectedFolder)
                    .Where(d => d.Exists)
                    .SelectMany(a => a.GetDirectories(Constants.FolderSystem)
                    ).Union(s.GetDirectories(Constants.FolderOldDotData)));
        var paths = subDirs.Where(d => d.Exists).Select(s => s.FullName).ToList();
        return l.Return(paths, $"OK, paths:{string.Join(";", paths)}");
    }

    #endregion
}