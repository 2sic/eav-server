using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Apps.Paths;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Repositories;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Run
{
    public class AppFileSystemLoader: ServiceBase, IAppFileSystemLoader, IAppRepositoryLoader
    {
        #region Constants

        public const string FieldFolderPrefix = "field-";
        public const string JsFile = "index.js";

        #endregion

        #region Dependencies and Constructor

        public class Dependencies: ServiceDependencies
        {
            public Dependencies(ISite site, Generator<FileSystemLoader> fslGenerator, LazySvc<AppPaths> appPathsLazy)
            {
                AddToLogQueue(
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
        /// <param name="deps"></param>
        public AppFileSystemLoader(Dependencies deps) : this(deps, EavLogs.Eav + ".AppFSL") { }

        /// <summary>
        /// Inheritance constructor
        /// </summary>
        /// <param name="deps"></param>
        /// <param name="logName"></param>
        protected AppFileSystemLoader(Dependencies deps, string logName) : base(logName)
        {
            Deps = deps.SetLog(Log);
            Site = deps.Site;
        }
        protected readonly Dependencies Deps;

        #endregion

        public string Path { get; set; }
        public string PathShared { get; set; }
        protected int AppId => _appState.AppId;
        protected ISite Site;
        private AppState _appState;
        private AppPaths _appPaths;

        #region Inits

        public IAppFileSystemLoader Init(AppState app) => Log.Func($"{app.AppId}, {app.Folder}, ...", () =>
        {
            _appState = app;
            _appPaths = Deps.AppPathsLazy.Value?.Init(Site, app);
            InitPathAfterAppId();
            return this;
        });

        IAppRepositoryLoader IAppRepositoryLoader.Init(AppState app) => Init(app) as IAppRepositoryLoader;

        /// <summary>
        /// Init Path After AppId must be in an own method, as each implementation may have something custom to handle this
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected virtual bool InitPathAfterAppId() => Log.Func(() =>
        {
            Path = System.IO.Path.Combine(_appPaths.PhysicalPath, Constants.FolderAppExtensions);
            PathShared = System.IO.Path.Combine(_appPaths.PhysicalPathShared, Constants.FolderAppExtensions);
            return (true, $"p:{Path}, ps:{PathShared}");
        });

        #endregion



        /// <inheritdoc />
        public List<InputTypeInfo> InputTypes() => Log.Func(() =>
        {
            var types = GetInputTypes(Path, AppConstants.AppPathPlaceholder);
            types.AddRange(GetInputTypes(PathShared, AppConstants.AppPathSharedPlaceholder));
            return (types, $"{types.Count}");
        });

        /// <inheritdoc />
        public IList<IContentType> ContentTypes(IEntitiesSource entitiesSource) => Log.Func(l =>
        {
            try
            {
                var extPaths = ExtensionPaths();
                l.A($"Found {extPaths.Count} extensions with .data folder");
                var allTypes = extPaths.SelectMany(p => LoadTypesFromOneExtensionPath(p, entitiesSource))
                    .Distinct(new EqualityComparer_ContentType())
                    .ToList();
                return (allTypes, "ok");
            }
            catch (Exception e)
            {
                l.A("error " + e.Message);
            }

            return (new List<IContentType>(), "error");
        });


        private IEnumerable<IContentType> LoadTypesFromOneExtensionPath(string extensionPath, IEntitiesSource entitiesSource) => Log.Func(extensionPath, () =>
        {
            var fsLoader = Deps.FslGenerator.New().Init(AppId, extensionPath, RepositoryTypes.Folder, true, entitiesSource);
            var types = fsLoader.ContentTypes();
            return types;
        });



        #region Helpers

        private List<InputTypeInfo> GetInputTypes(string path, string placeholder) => Log.Func(() =>
        {
            var di = new DirectoryInfo(path);
            if (!di.Exists) return (new List<InputTypeInfo>(), "directory not found");
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
                        $"{placeholder}/{Eav.Constants.FolderAppExtensions}/{name}/{JsFile}", false);
                })
                .ToList();
            return (types, $"{types.Count}");
        });

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
            var dir = new DirectoryInfo(Path);
            if (!dir.Exists) return new List<string>();
            var sub = dir.GetDirectories();
            var subDirs = sub.SelectMany(
                s => s.GetDirectories(System.IO.Path.Combine(Constants.AppDataProtectedFolder, Constants.FolderData))
                    .Union(s.GetDirectories(".data"))
                );
            var paths = subDirs.Select(s => s.FullName).ToList();
            return paths;
        }

        #endregion
    }
}
