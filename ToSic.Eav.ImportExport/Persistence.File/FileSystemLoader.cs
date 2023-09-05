using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Source;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.ImportExport.Serialization;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repositories;
using ToSic.Lib.DI;
using ToSic.Lib.Helpers;
using ToSic.Lib.Services;
using static ToSic.Eav.ImportExport.ImpExpConstants;
using IEntity = ToSic.Eav.Data.IEntity;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Persistence.File
{
    public partial class FileSystemLoader: ServiceBase, IContentTypeLoader
    {
        private readonly Generator<JsonSerializer> _jsonSerializerGenerator;
        private readonly DataBuilder _dataBuilder;
        public int AppId = -999;

        /// <summary>
        /// Empty constructor for DI
        /// </summary>
        public FileSystemLoader(Generator<JsonSerializer> jsonSerializerGenerator, DataBuilder dataBuilder) : base($"{EavLogs.Eav}.FsLoad")
        {
            ConnectServices(
                _jsonSerializerGenerator = jsonSerializerGenerator,
                _dataBuilder = dataBuilder
            );
        }


        public FileSystemLoader Init(int appId, string path, RepositoryTypes repoType, bool ignoreMissing, IEntitiesSource entitiesSource)
        {
            var l = Log.Fn<FileSystemLoader>($"init with appId:{appId}, path:{path}, ignore:{ignoreMissing}");
            AppId = appId;
            Path = path + (path.EndsWith("\\") ? "" : "\\");
            RepoType = repoType;
            IgnoreMissingStuff = ignoreMissing;
            EntitiesSource = entitiesSource;
            return l.ReturnAsOk(this);
        }

        private string Path { get; set; }

        private bool IgnoreMissingStuff { get; set; }

        private RepositoryTypes RepoType { get; set; }

        protected IEntitiesSource EntitiesSource;

        #region json serializer
        public JsonSerializer Serializer
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

        public IList<IEntity> Entities(string folder, int idSeed, DirectEntitiesSource relationships)
        {
            var l = Log.Fn<IList<IEntity>>($"Entities in {folder} with idSeed:{idSeed}");
            
            // #1. check that folder exists
            var subPath = System.IO.Path.Combine(Path, folder);
            if (!CheckPathExists(Path) || !CheckPathExists(subPath))
                return l.Return(new List<IEntity>(), "Path doesn't exist, return none");

            // #2. WIP - Allow relationships between loaded items
            // If we are loading from a larger context, then we have a reference to a list
            // which will be repopulated later, so only create a new one if there is none
            //var hasOwnRelationshipList = relationships == null;
            //l.A("hasOwnRelationshipList: " + hasOwnRelationshipList);
            //relationships = relationships ?? new List<IEntity>();
            var relationshipsSource = relationships;// new DirectEntitiesSource(relationships);

            // #3A. special case for entities in bundles.
            if (folder == FsDataConstants.BundlesFolder)
            {
                // #3A.1 load entities from files in bundles folder
                var entitiesInBundle = EntitiesInBundles(relationshipsSource).ToList();
                l.A($"Found {entitiesInBundle.Count} Entities in Bundles");

                // #3A.2 put all found entities into the source
                //if (hasOwnRelationshipList)
                //    relationships.AddRange(entitiesInBundle);

                return l.Return(entitiesInBundle, $"{entitiesInBundle.Count}");
            }

            // #3. older case with single entities
            // #3.1 find all content-type files in folder
            var jsons = Directory
                .GetFiles(subPath, $"*{Extension(Files.json)}")
                .OrderBy(f => f)
                .ToArray();

            // #3.2 load entity-items from folder
            var jsonSerializer = Serializer;
            var entities = jsons
                .Select(json => LoadAndBuildEntity(jsonSerializer, json, ++idSeed, relationshipsSource))
                .Where(entity => entity != null)
                .ToList();
            l.A("found " + entities.Count + " entities in " + folder + " folder");

            // #3.3 put all found entities into the source
            //if (hasOwnRelationshipList)
            //    relationships.AddRange(entities);

            return l.Return(entities, $"{entities.Count}");
        }

        #endregion


        #region ContentType

        public int EntityIdSeed = -1;
        public int TypeIdSeed = -1;

        public IList<IContentType> ContentTypes() => ContentTypes(AppId, null);

        /// <inheritdoc />
        /// <param name="appId">this is not used ATM - just for interface compatibility, must always be 0</param>
        /// <param name="source">this is not used ATM - just for interface compatibility</param>
        /// <returns></returns>
        public IList<IContentType> ContentTypes(int appId, IHasMetadataSource source)
        {
            var l = Log.Fn<IList<IContentType>>($"ContentTypes in {appId}");
            
            // #1. check that folder exists
            var pathCt = ContentTypePath;
            var contentTypes = new List<IContentType>();
            if (CheckPathExists(Path) && CheckPathExists(pathCt))
            {
                // #2 find all content-type files in folder
                var jsons = Directory.GetFiles(pathCt, "*" + Extension(Files.json)).OrderBy(f => f);

                // #3 load content-types from folder
                contentTypes = jsons
                    .Select(json => LoadAndBuildCt(Serializer, json))
                    .Where(ct => ct != null)
                    .ToList();
            }
            else
                l.A("path doesn't exist");

            var entityCtCount = contentTypes.Count;

            // #4 load content-types from files in bundles folder
            var bundleCts = ContentTypesInBundles();
            var bundleCtsWithoutDuplicates = bundleCts
                .Where(bundleCt => !contentTypes.Any(ct => ct.Is(bundleCt.NameId)))
                .ToList();
            contentTypes.AddRange(bundleCtsWithoutDuplicates);
            l.A($"Types in Entities: {entityCtCount}; in Bundles {bundleCts.Count}; after remove duplicates {bundleCtsWithoutDuplicates.Count}; total {contentTypes.Count}");
            return l.Return(contentTypes, $"{contentTypes.Count}");
        }

        private string ContentTypePath => System.IO.Path.Combine(Path, FsDataConstants.TypesFolder);

        /// <summary>
        /// Try to load a content-type file, but if anything fails, just return a null
        /// </summary>
        /// <returns></returns>
        private IContentType LoadAndBuildCt(JsonSerializer ser, string path)
        {
            var l = Log.Fn<IContentType>($"Path: {path}");
            var infoIfError = "couldn't read type-file";
            try
            {
                var json = System.IO.File.ReadAllText(path);

                infoIfError = "couldn't deserialize string";
                var ct = ser.DeserializeContentType(json);

                infoIfError = "couldn't set source/parent";
                ct = _dataBuilder.ContentType.CreateFrom(ct, id: ++TypeIdSeed, repoType: RepoType, parentTypeId: Constants.PresetContentTypeFakeParent, repoAddress: path);
                return l.ReturnAsOk(ct);
            }
            catch (IOException e)
            {
                l.Ex("Failed loading type - couldn't import type-file, IO exception", e);
                return l.ReturnNull();
            }
            catch (Exception e)
            {
                l.Ex($"Failed loading type - {infoIfError}", e);
                return l.ReturnNull();
            }
        }
        #endregion

        #region Bundle

        public Dictionary<string, JsonFormat> JsonBundleBundles => _jsonBundles.GetM(Log, l =>
        {
            // #1. check that folder exists
            if (!CheckPathExists(Path) || !CheckPathExists(BundlesPath))
                return (new Dictionary<string, JsonFormat>(), "path doesn't exist");

            const string infoIfError = "couldn't read bundle-file";
            try
            {
                // #2 find all bundle files in folder and unpack/deserialize to JsonFormat
                var jsonBundles = new Dictionary<string, JsonFormat>();
                Directory.GetFiles(BundlesPath, "*" + Extension(Files.json)).OrderBy(f => f).ToList()
                    .ForEach(p =>
                    {
                        l.A("Loading json bundle" + p);
                        jsonBundles[p] = Serializer.UnpackAndTestGenericJsonV1(System.IO.File.ReadAllText(p));
                    });
                return (jsonBundles, $"found {jsonBundles.Count}");
            }
            catch (IOException e)
            {
                l.Ex("Failed loading type - couldn't import bundle-file, IO exception", e);
                return (new Dictionary<string, JsonFormat>(), "IOException");
            }
            catch (Exception e)
            {
                l.Ex($"Failed loading bundle - {infoIfError}", e);
                return (new Dictionary<string, JsonFormat>(), "error");
            }
        });
        private readonly GetOnce<Dictionary<string, JsonFormat>> _jsonBundles = new GetOnce<Dictionary<string, JsonFormat>>();

        public List<IContentType> ContentTypesInBundles()
        {
            var l = Log.Fn<List<IContentType>>($"ContentTypes in bundles");
            if (JsonBundleBundles.All(jb => jb.Value.Bundles?.Any(b => b.ContentTypes.SafeAny()) != true))
                return new List<IContentType>();

            var contentTypes = JsonBundleBundles
                .SelectMany(json => BuildContentTypesInBundles(Serializer, json.Key, json.Value /*, relationshipsList*/))
                .Where(ct => ct != null).ToList();

            l.A("ContentTypes in bundles: " + contentTypes.Count);

            return l.ReturnAsOk(contentTypes);
        }

        public List<IEntity> EntitiesInBundles(IEntitiesSource relationshipSource)
        {
            var l = Log.Fn<List<IEntity>>($"Entities in bundles");
            if (JsonBundleBundles.All(jb => jb.Value.Bundles?.Any(b => b.Entities.SafeAny()) != true))
                return l.Return(new List<IEntity>(), "no bundles have entities, return none");

            var entities = JsonBundleBundles
                .SelectMany(json =>
                    BuildEntitiesInBundles(Serializer, json.Key, json.Value, relationshipSource))
                .Where(entity => entity != null).ToList();

            return l.Return(entities, $"Entities in bundles: {entities.Count}");
        }
        
        private string BundlesPath => System.IO.Path.Combine(Path, FsDataConstants.BundlesFolder);

        /// <summary>
        /// Build contentTypes from bundle json
        /// </summary>
        /// <returns></returns>
        private List<IContentType> BuildContentTypesInBundles(JsonSerializer ser, string path, JsonFormat bundleJson)
        {
            var l = Log.Fn<List<IContentType>>($"path: {path}");
            try
            {
                var contentTypes = ser.GetContentTypesFromBundles(bundleJson);

                var newContentTypes = contentTypes
                    .Select(ct => _dataBuilder.ContentType.CreateFrom(ct, id: ++TypeIdSeed,
                        repoType: RepoType, repoAddress: path,
                        parentTypeId: Constants.PresetContentTypeFakeParent,
                        configZoneId: Constants.PresetZoneId,
                        configAppId: Constants.PresetAppId)
                    )
                    .ToList();

                return l.ReturnAsOk(newContentTypes);
            }
            catch (Exception e)
            {
                l.Ex($"Failed building content types from bundle json", e);
                return l.Return(new List<IContentType>(), "error");
            }
        }

        /// <summary>
        /// Build entities from bundle json
        /// </summary>
        /// <returns></returns>
        private List<IEntity> BuildEntitiesInBundles(JsonSerializer ser, string path, JsonFormat bundleJson, IEntitiesSource relationshipSource)
        {
            var l = Log.Fn<List<IEntity>>($"Build entities from bundle json: {path}.");
            try
            {
                // WIP - Allow relationships between loaded items
                // If we are loading from a larger context, then we have a reference to a list
                // which will be repopulated later, so only create a new one if there is none
                var entities = ser.GetEntitiesFromBundles(bundleJson, relationshipSource);
                entities = entities
                    .Select(e =>
                    {
                        var newId = ++EntityIdSeed;
                        return _dataBuilder.Entity.CreateFrom(e, id: newId, repositoryId: newId);
                    })
                    .ToList();
                //entities.ForEach(e => e.ResetEntityIdAll(++EntityIdSeed));
                return l.Return(entities, $"{entities.Count}");
            }
            catch (Exception e)
            {
                l.Ex("Failed building entities from bundle json", e);
                return l.Return(new List<IEntity>(), "error return none");
            }
        }

        #endregion



        #region todo someday items

        /// <summary>
        /// Try to load an entity (for example a query-definition)
        /// If anything fails, just return a null
        /// </summary>
        /// <returns></returns>
        private IEntity LoadAndBuildEntity(JsonSerializer ser, string path, int id, IEntitiesSource relationshipSource = null)
        {
            var l = Log.Fn<IEntity>($"Loading {path}");
            try
            {
                var json = System.IO.File.ReadAllText(path);
                var entity = ser.DeserializeWithRelsWip(json, id, allowDynamic: true, skipUnknownType: false, relationshipSource);
                return l.ReturnAsOk(entity);
            }
            catch (IOException e)
            {
                l.Ex($"Failed loading type - couldn't read file on '{path}'", e);
                return l.ReturnNull();
            }
            catch (Exception e)
            {
                l.Ex($"Failed loading type - couldn't deserialize '{path}' for unknown reason.", e);
                return l.ReturnNull();
            }
        }

        #endregion

        /// <summary>
        /// Check if a path exists - if missing path is forbidden, will raise error
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool CheckPathExists(string path)
        {
            var l = Log.Fn<bool>($"Check path exists: {path}");
            if (Directory.Exists(path)) return l.ReturnTrue("ok");
            if (!IgnoreMissingStuff)
                throw new DirectoryNotFoundException("directory '" + path + "' not found, and couldn't ignore");
            l.A("path: doesn't exist, but ignore");
            return l.ReturnFalse("not found");
        }

    }
}
