using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Serialization;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repositories;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using static ToSic.Eav.ImportExport.ImpExpConstants;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Persistence.File
{
    public partial class FileSystemLoader: ServiceBase, IContentTypeLoader
    {
        public int AppId = -999;

        /// <summary>
        /// Empty constructor for DI
        /// </summary>
        public FileSystemLoader(Generator<JsonSerializer> jsonSerializerGenerator) : base($"{EavLogs.Eav}.FsLoad")
        {
            ConnectServices(
                _jsonSerializerGenerator = jsonSerializerGenerator
            );
        }

        private readonly Generator<JsonSerializer> _jsonSerializerGenerator;

        public FileSystemLoader Init(int appId, string path, RepositoryTypes repoType, bool ignoreMissing, IEntitiesSource entitiesSource)
        {
            Log.A($"init with appId:{appId}, path:{path}, ignore:{ignoreMissing}");
            AppId = appId;
            Path = path + (path.EndsWith("\\") ? "" : "\\");
            RepoType = repoType;
            IgnoreMissingStuff = ignoreMissing;
            EntitiesSource = entitiesSource;
            return this;
        }

        private string Path { get; set; }

        private bool IgnoreMissingStuff { get; set; }

        private RepositoryTypes RepoType { get; set; }

        protected IEntitiesSource EntitiesSource;

        #region json serializer
        private JsonSerializer Serializer
        {
            get
            {
                if (_ser != null) return _ser;
                _ser = _jsonSerializerGenerator.New();
                _ser.Initialize(AppId, new List<IContentType>(), EntitiesSource);
                _ser.AssumeUnknownTypesAreDynamic = true;
                return _ser;
            }
        }
        private JsonSerializer _ser;

        internal void ResetSerializer(AppState appState)
        {
            var serializer = _jsonSerializerGenerator.New().SetApp(appState);
            _ser = serializer;
        }
        internal void ResetSerializer(List<IContentType> types)
        {
            var serializer = _jsonSerializerGenerator.New();
            serializer.Initialize(AppId, types, null);
            _ser = serializer;
        }

        #endregion

        #region Queries & Configuration

        public IList<IEntity> Entities(string folder, int idSeed, List<IEntity> relationshipsList = null)
        {
            // #1. check that folder exists
            var subPath = System.IO.Path.Combine(Path, folder);
            if (!CheckPathExists(Path) || !CheckPathExists(subPath))
                return new List<IEntity>();

            // #2 find all content-type files in folder
            var jsons = Directory
                .GetFiles(subPath, $"*{Extension(Files.json)}")
                .OrderBy(f => f)
                .ToArray();

            // #3.1 WIP - Allow relationships between loaded items
            // If we are loading from a larger context, then we have a reference to a list
            // which will be repopulated later, so only create a new one if there is none
            relationshipsList = relationshipsList ?? new List<IEntity>();
            var relationshipsSource = new DirectEntitiesSource(relationshipsList);

            // #3.2 load entity-items from folder
            var jsonSerializer = Serializer;
            var entities = jsons
                .Select(json => LoadAndBuildEntity(jsonSerializer, json, ++idSeed, relationshipsSource))
                .Where(entity => entity != null)
                .ToList();

            // #4 load entities from files in bundles folder
            entities.AddRange(EntitiesInBundles());

            return entities;
        }

        #endregion


        #region ContentType

        public int IdSeed = -1;

        public IList<IContentType> ContentTypes() => ContentTypes(AppId, null);

        /// <inheritdoc />
        /// <param name="appId">this is not used ATM - just for interface compatibility, must always be 0</param>
        /// <param name="source">this is not used ATM - just for interface compatibility</param>
        /// <returns></returns>
        public IList<IContentType> ContentTypes(int appId, IHasMetadataSource source)
        {
            // v11.01 experimental - maybe disable this, as now we're loading from the app folder so we have an AppId
            // 2021-12-06 2dm - disabled this - it prevented app-system-folder content-types from loading
            // if (appId != Constants.PresetAppId) throw new ArgumentOutOfRangeException(nameof(appId), appId, "appid should only be 0 for now");

            // #1. check that folder exists
            var pathCt = ContentTypePath;
            if (!CheckPathExists(Path) || !CheckPathExists(pathCt))
                return new List<IContentType>();

            // #2 find all content-type files in folder
            var jsons = Directory.GetFiles(pathCt, "*" + Extension(Files.json)).OrderBy(f => f);

            // #3 load content-types from folder
            var cts = jsons
                .Select(json => LoadAndBuildCt(Serializer, json, IdSeed == -1 ? 0 : IdSeed++))
                .Where(ct => ct != null).ToList();

            // #4 load content-types from files in bundles folder
            cts.AddRange(ContentTypesInBundles());

            return cts;
        }

        private string ContentTypePath => System.IO.Path.Combine(Path, Configuration.FsDataConstants.TypesFolder);

        /// <summary>
        /// Try to load a content-type file, but if anything fails, just return a null
        /// </summary>
        /// <returns></returns>
        private IContentType LoadAndBuildCt(JsonSerializer ser, string path, int id)
        {
            Log.A("Loading " + path);
            var infoIfError = "couldn't read type-file";
            try
            {
                var json = System.IO.File.ReadAllText(path);

                infoIfError = "couldn't deserialize string";
                var ct = ser.DeserializeContentType(json);

                infoIfError = "couldn't set source/parent";
                (ct as ContentType).SetSourceParentAndIdForPresetTypes(RepoType, Constants.PresetContentTypeFakeParent, path, id);
                return ct;
            }
            catch (IOException e)
            {
                Log.A("Failed loading type - couldn't import type-file, IO exception: " + e);
                return null;
            }
            catch (Exception e)
            {
                Log.A($"Failed loading type - {infoIfError}, exception '" + e.GetType().FullName + "':" + e.Message);
                return null;
            }
        }
        #endregion

        #region Bundle

        public Dictionary<string, string> JsonBundleBundles
        {
            get
            {
                if (_jsonBundles != null) return _jsonBundles;
                _jsonBundles = new Dictionary<string, string>();
                
                // #1. check that folder exists
                if (!CheckPathExists(Path) || !CheckPathExists(BundlesPath))
                    return _jsonBundles;

                // #2 find all bundle files in folder
                Directory.GetFiles(BundlesPath, "*" + Extension(Files.json)).OrderBy(f => f).ToList()
                    .ForEach(p => _jsonBundles[p] = System.IO.File.ReadAllText(p));

                return _jsonBundles;
            }
        }
        private Dictionary<string, string> _jsonBundles;

        public IList<IContentType> ContentTypesInBundles()
        {
            if (JsonBundleBundles.Any() == false) return new List<IContentType>();

            var bundles = JsonBundleBundles
                .SelectMany(json => LoadBundlesAndBuildContentTypes(Serializer, json.Key, json.Value, IdSeed == -1 ? 0 : IdSeed++))
                .Where(ct => ct != null).ToList();

            var contentTypesInBundles = bundles
                .Where(b => b.ContentTypes.Any() == true)
                .SelectMany(b => b.ContentTypes)
                .ToList();

            return contentTypesInBundles;
        }

        public IList<Entity> EntitiesInBundles(List<IEntity> relationshipsList = null)
        {
            if (JsonBundleBundles.Any() == false) return new List<Entity>();

            var bundles = JsonBundleBundles
                .SelectMany(json => LoadBundlesAndBuildEntities(Serializer, json.Value, IdSeed == -1 ? 0 : IdSeed++, relationshipsList))
                .Where(ct => ct != null).ToList();

            var entities = bundles
                .Where(b => b.Entities?.Any() == true)
                .SelectMany(b => b.Entities).ToList();

            // #3.3 Put all found entities into the source
            if (entities?.Any() == true) relationshipsList?.AddRange(entities);

            return entities;
        }

        private string BundlesPath => System.IO.Path.Combine(Path, Configuration.FsDataConstants.BundlesFolder);

        /// <summary>
        /// Try to load a bundle file, but if anything fails, just return a null
        /// </summary>
        /// <returns></returns>
        private List<Bundle> LoadBundlesAndBuildContentTypes(JsonSerializer ser, string path, string json, int id) => Log.Func(l =>
        {
            l.A($"Loading bundles and building content-types from json:{json.Length}");
            const string infoIfError = "couldn't read bundle-file";
            try
            {
                var bundleList = ser.DeserializeContentTypes(json);

                foreach (var contentType in bundleList
                             .Where(bundle => bundle.ContentTypes?.Any() == true)
                             .SelectMany(bundle => bundle.ContentTypes))
                    (contentType as ContentType).SetSourceParentAndIdForPresetTypes(RepoType,
                        Constants.PresetContentTypeFakeParent, path, ++id);

                return bundleList;
            }
            catch (IOException e)
            {
                l.A("Failed loading type - couldn't import bundle-file, IO exception: " + e);
                return null;
            }
            catch (Exception e)
            {
                l.A($"Failed loading bundle - {infoIfError}, exception '" + e.GetType().FullName + "':" + e.Message);
                return null;
            }
        });

        /// <summary>
        /// Try to load a bundle file, but if anything fails, just return a null
        /// </summary>
        /// <returns></returns>
        private List<Bundle> LoadBundlesAndBuildEntities(JsonSerializer ser, string json, int id, List<IEntity> relationshipsList = null
        ) => Log.Func(l =>
        {
            l.A($"Loading bundles and build entities from json:{json.Length}");
            const string infoIfError = "couldn't read bundle-file";
            try
            {
                // #3.1 WIP - Allow relationships between loaded items
                // If we are loading from a larger context, then we have a reference to a list
                // which will be repopulated later, so only create a new one if there is none
                relationshipsList = relationshipsList ?? new List<IEntity>();
                var relationshipsSource = new DirectEntitiesSource(relationshipsList);

                var bundleList = ser.DeserializeEntities(json, id, relationshipsSource);

                return bundleList;
            }
            catch (IOException e)
            {
                l.A("Failed loading type - couldn't import bundle-file, IO exception: " + e);
                return null;
            }
            catch (Exception e)
            {
                l.A($"Failed loading bundle - {infoIfError}, exception '" + e.GetType().FullName + "':" + e.Message);
                return null;
            }
        });

        #endregion



        #region todo someday items

        /// <summary>
        /// Try to load an entity (for example a query-definition)
        /// If anything fails, just return a null
        /// </summary>
        /// <returns></returns>
        private IEntity LoadAndBuildEntity(JsonSerializer ser, string path, int id, IEntitiesSource relationshipSource = null) => Log.Func(l =>
        {
            l.A("Loading " + path);
            try
            {
                var json = System.IO.File.ReadAllText(path);
                var entity = ser.DeserializeWithRelsWip(json, id, allowDynamic: true, skipUnknownType: false, relationshipSource);
                return entity;
            }
            catch (IOException e)
            {
                l.A($"Failed loading type - couldn't read file on '{path}'");
                l.Ex(e);
                return null;
            }
            catch (Exception e)
            {
                l.A($"Failed loading type - couldn't deserialize '{path}' for unknown reason.");
                l.Ex(e);
                return null;
            }
        });

        #endregion

        /// <summary>
        /// Check if a path exists - if missing path is forbidden, will raise error
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool CheckPathExists(string path) => Log.Func($"path: check exists '{path}'", l =>
        {
            if (Directory.Exists(path)) return (true, "ok");
            if (!IgnoreMissingStuff)
                throw new DirectoryNotFoundException("directory '" + path + "' not found, and couldn't ignore");
            l.A("path: doesn't exist, but ignore");
            return (false, "not found");
        });

    }
}
