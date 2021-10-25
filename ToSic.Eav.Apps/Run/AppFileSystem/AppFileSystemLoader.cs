using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.Apps.Run
{
    public class AppFileSystemLoader: HasLog, IAppFileSystemLoader, IAppRepositoryLoader
    {
        #region Constants

        public const string FieldFolderPrefix = "field-";
        public const string JsFile = "index.js";

        #endregion

        #region Dependencies and Constructor

        public class Dependencies
        {
            public Dependencies(IServiceProvider serviceProvider, ISite site)
            {
                ServiceProvider = serviceProvider;
                Site = site;
            }
            public IServiceProvider ServiceProvider { get; }
            public ISite Site { get; }
        }

        /// <summary>
        /// DI Constructor
        /// </summary>
        /// <param name="deps"></param>
        public AppFileSystemLoader(Dependencies deps) : this(deps, LogNames.Eav + ".AppFSL") { }

        /// <summary>
        /// Inheritance constructor
        /// </summary>
        /// <param name="deps"></param>
        /// <param name="logName"></param>
        protected AppFileSystemLoader(Dependencies deps, string logName) : base(logName)
        {
            Deps = deps;
            Site = deps.Site;
        }
        protected readonly Dependencies Deps;

        #endregion

        protected int AppId { get; set; }

        public string Path { get; set; }

        protected ISite Site;

        #region Inits

        public IAppFileSystemLoader Init(int appId, string path, ILog log)
        {
            Log.LinkTo(log);

            var wrapLog = Log.Call($"{appId}, {path}, ...");
            AppId = appId;
            InitPathAfterAppId(path);

            wrapLog(null);
            return this;
        }

        IAppRepositoryLoader IAppRepositoryLoader.Init(int appId, string path, ILog log) => Init(appId, path, log) as IAppRepositoryLoader;

        /// <summary>
        /// Init Path After AppId must be in an own method, as each implementation may have something custom to handle this
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected virtual bool InitPathAfterAppId(string path)
        {
            var wrapLog = Log.Call<bool>(path);
            Path = System.IO.Path.Combine(Site.AppsRootPhysicalFull, path, Eav.Constants.FolderAppExtensions);
            return wrapLog(Path, true);
        }


        #endregion



        /// <inheritdoc />
        public List<InputTypeInfo> InputTypes()
        {
            var wrapLog = Log.Call<List<InputTypeInfo>>();
            var di = new DirectoryInfo(Path);
            if (!di.Exists) return wrapLog("directory not found", new List<InputTypeInfo>());
            var inputFolders = di.GetDirectories(FieldFolderPrefix + "*");
            Log.Add($"found {inputFolders.Length} field-directories");

            var withIndexJs = inputFolders
                .Where(fld => fld.GetFiles(JsFile).Any())
                .Select(fld => fld.Name).ToArray();
            Log.Add($"found {withIndexJs.Length} folders with {JsFile}");

            var types = withIndexJs.Select(name =>
                {
                    var fullName = name.Substring(FieldFolderPrefix.Length);
                    var niceName = NiceName(name);
                    // TODO: use metadata information if available
                    return new InputTypeInfo(fullName, niceName, "Extension Field", "", false,
                        $"{AppConstants.AppPathPlaceholder}/{Eav.Constants.FolderAppExtensions}/{name}/{JsFile}", false);
                })
                .ToList();
            return wrapLog(null, types);
        }

        /// <inheritdoc />
        public IList<IContentType> ContentTypes(IEntitiesSource entitiesSource)
        {
            var wrapLog = Log.Call<IList<IContentType>>();
            try
            {
                var extPaths = ExtensionPaths();
                Log.Add($"Found {extPaths.Count} extensions with .data folder");
                var allTypes = extPaths.SelectMany(p => LoadTypesFromOneExtensionPath(p, entitiesSource))
                    .Distinct(new EqualityComparer_ContentType())
                    .ToList();
                return wrapLog("ok", allTypes);
            }
            catch (Exception e)
            {
                Log.Add("error " + e.Message);
            }

            return wrapLog("error", new List<IContentType>());
        }


        private IEnumerable<IContentType> LoadTypesFromOneExtensionPath(string extensionPath, IEntitiesSource entitiesSource)
        {
            var wrapLog = Log.Call<IList<IContentType>>(extensionPath);
            var fsLoader = Deps.ServiceProvider.Build<FileSystemLoader>()
                .Init(extensionPath, RepositoryTypes.Folder, true, entitiesSource, Log);
            var types = fsLoader.ContentTypes();
            return wrapLog("ok", types);
        }



        #region Helpers

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
            var subDirs = sub.SelectMany(s => s.GetDirectories(Eav.Constants.FolderData));
            var paths = subDirs.Select(s => s.FullName).ToList();
            return paths;
        }

        #endregion
    }
}
