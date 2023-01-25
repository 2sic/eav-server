using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Json.V1;
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
            => Log.Func(l =>
            {
                // #1. check that folder exists
                var subPath = System.IO.Path.Combine(Path, folder);
                if (!CheckPathExists(Path) || !CheckPathExists(subPath))
                    return new List<IEntity>();

                // #2. WIP - Allow relationships between loaded items
                // If we are loading from a larger context, then we have a reference to a list
                // which will be repopulated later, so only create a new one if there is none
                var hasOwnRelationshipList = relationshipsList == null;
                l.A("hasOwnRelationshipList: " + hasOwnRelationshipList);
                relationshipsList = relationshipsList ?? new List<IEntity>();
                var relationshipsSource = new DirectEntitiesSource(relationshipsList);

                // #3A. special case for entities in bundles.
                if (folder == FsDataConstants.BundlesFolder)
                {
                    // #3A.1 load entities from files in bundles folder
                    var entitiesInBundle = EntitiesInBundles(relationshipsSource).ToList();
                    l.A("found " + entitiesInBundle.Count);

                    // #3A.2 put all found entities into the source
                    if (hasOwnRelationshipList)
                        relationshipsList.AddRange(entitiesInBundle);

                    return entitiesInBundle;
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
                if (hasOwnRelationshipList)
                    relationshipsList.AddRange(entities);

                return entities;
            });

        #endregion


        #region ContentType

        public int IdSeed = -1;

        public IList<IContentType> ContentTypes() => ContentTypes(AppId, null);

        /// <inheritdoc />
        /// <param name="appId">this is not used ATM - just for interface compatibility, must always be 0</param>
        /// <param name="source">this is not used ATM - just for interface compatibility</param>
        /// <returns></returns>
        public IList<IContentType> ContentTypes(int appId, IHasMetadataSource source) => Log.Func<IList<IContentType>>(l =>
        {
            // #1. check that folder exists
            var pathCt = ContentTypePath;
            if (!CheckPathExists(Path) || !CheckPathExists(pathCt))
                return (new List<IContentType>(), "path doesn't exist");

            // #2 find all content-type files in folder
            var jsons = Directory.GetFiles(pathCt, "*" + Extension(Files.json)).OrderBy(f => f);

            // #3 load content-types from folder
            var contentTypes = jsons
                .Select(json => LoadAndBuildCt(Serializer, json, IdSeed == -1 ? 0 : IdSeed++))
                .Where(ct => ct != null)
                .ToList();
            var entityCtCount = contentTypes.Count;

            // #4 load content-types from files in bundles folder
            var bundleCts = ContentTypesInBundles();
            var bundleCtsWithoutDuplicates = bundleCts
                .Where(bundleCt => !contentTypes.Any(ct => ct.Is(bundleCt.NameId)))
                .ToList();
            contentTypes.AddRange(bundleCtsWithoutDuplicates);
            l.A($"Types in Entities: {entityCtCount}; in Bundles {bundleCts.Count}; after remove duplicates {bundleCtsWithoutDuplicates.Count}; total {contentTypes.Count}");
            return (contentTypes, $"{contentTypes.Count}");
        });

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

        public Dictionary<string, JsonFormat> JsonBundleBundles
        {
            get
            {
                if (_jsonBundles != null) return _jsonBundles;
                _jsonBundles = Log.Func(l =>
                {
                    var jsonBundles = new Dictionary<string, JsonFormat>();

                    // #1. check that folder exists
                    if (!CheckPathExists(Path) || !CheckPathExists(BundlesPath))
                        return jsonBundles;

                    const string infoIfError = "couldn't read bundle-file";
                    try
                    {
                        // #2 find all bundle files in folder and unpack/deserialize to JsonFormat
                        Directory.GetFiles(BundlesPath, "*" + Extension(Files.json)).OrderBy(f => f).ToList()
                            .ForEach(p =>
                            {
                                l.A("Loading json bundle" + p);
                                jsonBundles[p] = Serializer.UnpackAndTestGenericJsonV1(System.IO.File.ReadAllText(p));
                            });
                    }
                    catch (IOException e)
                    {
                        l.A("Failed loading type - couldn't import bundle-file, IO exception: " + e);
                        return new Dictionary<string, JsonFormat>();
                    }
                    catch (Exception e)
                    {
                        l.A($"Failed loading bundle - {infoIfError}, exception '" + e.GetType().FullName + "':" +
                            e.Message);
                        return new Dictionary<string, JsonFormat>();
                    }

                    return jsonBundles;
                });
                return _jsonBundles;
            }
        }
        private Dictionary<string, JsonFormat> _jsonBundles;

        public List<IContentType> ContentTypesInBundles() =>
            Log.Func(l =>
            {
                if (JsonBundleBundles.All(jb => jb.Value.Bundles?.Any(b => b.ContentTypes?.Any() == true) != true))
                    return new List<IContentType>();

                var contentTypes = JsonBundleBundles
                    .SelectMany(json => BuildContentTypesInBundles(Serializer, json.Key, json.Value,
                        IdSeed == -1 ? 0 : IdSeed++ /*, relationshipsList*/))
                    .Where(ct => ct != null).ToList();

                l.A("ContentTypes in bundles: " + contentTypes.Count);

                return contentTypes;
            });

        public List<IEntity> EntitiesInBundles(IEntitiesSource relationshipSource) =>
            Log.Func(l =>
            {
                if (JsonBundleBundles.All(jb => jb.Value.Bundles?.Any(b => b.Entities?.Any() == true) != true))
                    return new List<IEntity>();

                var entities = JsonBundleBundles
                    .SelectMany(json =>
                        BuildEntitiesInBundles(Serializer, json.Key, json.Value, IdSeed == -1 ? 0 : IdSeed++, relationshipSource))
                    .Where(entity => entity != null).ToList();

                l.A("Entities in bundles: " + entities.Count);

                return entities;
            });
        
        private string BundlesPath => System.IO.Path.Combine(Path, Configuration.FsDataConstants.BundlesFolder);

        /// <summary>
        /// Build contentTypes from bundle json
        /// </summary>
        /// <returns></returns>
        private List<IContentType> BuildContentTypesInBundles(JsonSerializer ser, string path, JsonFormat bundleJson, int id) => Log.Func(l =>
        {
            l.A($"Building content-types from bundle json: {path}");
            try
            {
                var contentTypes = ser.BundleContentTypes(bundleJson);


                contentTypes.ForEach(contentType =>
                {
                    (contentType as ContentType).SetSourceParentAndIdForPresetTypes(RepoType,
                        Constants.PresetContentTypeFakeParent, path, ++id);
                });

                return contentTypes;
            }
            catch (Exception e)
            {
                l.A($"Failed building content types from bundle json, exception '" + e.GetType().FullName + "':" + e.Message);
                return new List<IContentType>();
            }
        });

        /// <summary>
        /// Build entities from bundle json
        /// </summary>
        /// <returns></returns>
        private List<IEntity> BuildEntitiesInBundles(JsonSerializer ser, string path, JsonFormat bundleJson, int id, IEntitiesSource relationshipSource = null
        ) => Log.Func(l =>
        {
            l.A($"Build entities from bundle json: {path}.");
            try
            {
                // WIP - Allow relationships between loaded items
                // If we are loading from a larger context, then we have a reference to a list
                // which will be repopulated later, so only create a new one if there is none
                relationshipSource = relationshipSource ?? new DirectEntitiesSource(new List<IEntity>());
                var entities = ser.BundleEntities(bundleJson, relationshipSource);
                entities?.ForEach(e => e.ResetEntityIdAll(++id));
                return entities;
            }
            catch (Exception e)
            {
                l.A($"Failed building entities from bundle json, exception '" + e.GetType().FullName + "':" + e.Message);
                return new List<IEntity>();
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
