using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport.Json;
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
        public FileSystemLoader(GeneratorLog<JsonSerializer> jsonSerGenerator) : base($"{LogNames.Eav}.FsLoad")
            => ConnectServices(_jsonSerGenerator = jsonSerGenerator);

        private readonly GeneratorLog<JsonSerializer> _jsonSerGenerator;

        public FileSystemLoader Init(int appId, string path, RepositoryTypes repoType, bool ignoreMissing, IEntitiesSource entitiesSource, ILog parentLog)
        {
            this.Init(parentLog);
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
                _ser = _jsonSerGenerator.New();
                _ser.Initialize(AppId, new List<IContentType>(), EntitiesSource, Log);
                _ser.AssumeUnknownTypesAreDynamic = true;
                return _ser;
            }
        }
        private JsonSerializer _ser;

        internal void ResetSerializer(AppState appState)
        {
            var serializer = _jsonSerGenerator.New();
            serializer.Init(appState, Log);
            _ser = serializer;
        }
        internal void ResetSerializer(List<IContentType> types)
        {
            var serializer = _jsonSerGenerator.New();
            serializer.Initialize(AppId, types, null, Log);
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

            // #3.3 Put all found entities into the source
            relationshipsList.AddRange(entities);
            
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


        #region todo someday items

        /// <summary>
        /// Try to load an entity (for example a query-definition)
        /// If anything fails, just return a null
        /// </summary>
        /// <returns></returns>
        private IEntity LoadAndBuildEntity(JsonSerializer ser, string path, int id, IEntitiesSource relationshipSource = null)
        {
            Log.A("Loading " + path);
            try
            {
                var json = System.IO.File.ReadAllText(path);
                var entity = ser.DeserializeWithRelsWip(json, id, allowDynamic: true, skipUnknownType: false, relationshipSource);
                return entity;
            }
            catch (IOException e)
            {
                Log.A($"Failed loading type - couldn't read file on '{path}'");
                Log.Ex(e);
                return null;
            }
            catch (Exception e)
            {
                Log.A($"Failed loading type - couldn't deserialize '{path}' for unknown reason.");
                Log.Ex(e);
                return null;
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
            var wrapLog = Log.Fn<bool>("path: check exists '" + path + "'");
            if (Directory.Exists(path)) return wrapLog.ReturnTrue("ok");
            if (!IgnoreMissingStuff)
                throw new DirectoryNotFoundException("directory '" + path + "' not found, and couldn't ignore");
            Log.A("path: doesn't exist, but ignore");
            return wrapLog.ReturnFalse("not found");
        }

    }
}
