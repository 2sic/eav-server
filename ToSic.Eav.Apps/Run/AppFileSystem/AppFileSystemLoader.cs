using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Apps.Paths;
using ToSic.Eav.Apps.Work;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Source;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Repositories;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Run
{
    public class AppFileSystemLoader: ServiceBase<AppFileSystemLoader.MyServices>, IAppFileSystemLoader, IAppRepositoryLoader
    {
        #region Constants

        public const string FieldFolderPrefix = "field-";
        public const string JsFile = "index.js";

        #endregion

        #region Dependencies and Constructor

        public class MyServices: MyServicesBase
        {
            public MyServices(ISite site, Generator<FileSystemLoader> fslGenerator, LazySvc<AppPaths> appPathsLazy)
            {
                ConnectServices(
                    Site = site,
                    FslGenerator = fslGenerator,
                    AppPathsLazy = appPathsLazy
                );
            }
            public ISite Site { get; }
            internal Generator<FileSystemLoader> FslGenerator { get; }
            internal LazySvc<AppPaths> AppPathsLazy { get; }
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
        protected int AppId => _appState.AppId;
        protected ISite Site;
        private AppState _appState;
        private AppPaths _appPaths;

        #region Inits

        public IAppFileSystemLoader Init(AppState app)
        {
            var l = Log.Fn<IAppFileSystemLoader>($"{app.AppId}, {app.Folder}, ...");
            _appState = app;
            _appPaths = Services.AppPathsLazy.Value?.Init(Site, app);
            InitPathAfterAppId();
            return l.Return(this);
        }

        IAppRepositoryLoader IAppRepositoryLoader.Init(AppState app) => Init(app) as IAppRepositoryLoader;

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
            var fsLoader = base.Services.FslGenerator.New().Init(AppId, extensionPath, RepositoryTypes.Folder, true, entitiesSource);
            var types = fsLoader.ContentTypes();
            return l.Return(types);
        }



        #region Helpers

        private List<InputTypeInfo> GetInputTypes(string path, string placeholder)
        {
            var l = Log.Fn<List<InputTypeInfo>>();
            var di = new DirectoryInfo(path);
            if (!di.Exists) return l.Return(new List<InputTypeInfo>(), "directory not found");
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
            if (!dir.Exists) return l.Return(new List<string>(), $"directory do not exist: {dir}");
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
}
