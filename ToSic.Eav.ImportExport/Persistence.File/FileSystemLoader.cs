using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Repositories;
using ToSic.Eav.Types;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File
{
    public partial class FileSystemLoader: HasLog, IContentTypeLoader
    {
        private const string ContentTypeFolder = "contenttypes\\";
        private const string QueryFolder = "queries\\";
        private const string ConfigurationFolder = "configurations\\";
        //private const string ItemFolder = "items\\";


        public int AppId = 0;

        /// <summary>
        /// Empty constructor for DI
        /// </summary>
        public FileSystemLoader(JsonSerializer jsonSerializerUnready) : base("FSL.Loadr")
        {
            _jsonSerializerUnready = jsonSerializerUnready;
        }

        public FileSystemLoader Init(int appId, string path, RepositoryTypes repoType, bool ignoreMissing, IEntitiesSource entitiesSource, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            Log.Add($"init with appId:{appId}, path:{path}, ignore:{ignoreMissing}");
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
                _ser = _jsonSerializerUnready;
                _ser.Initialize(AppId, ReflectionTypes.FakeCache.Values, EntitiesSource, Log);
                _ser.AssumeUnknownTypesAreDynamic = true;
                return _ser;
            }
        }
        private JsonSerializer _ser;
        private readonly JsonSerializer _jsonSerializerUnready;

        #endregion

        #region Queries & Configuration
        private string QueryPath => Path + QueryFolder;
        private string ConfigurationPath => Path + ConfigurationFolder;
        public IList<IEntity> Queries() => LoadEntitiesFromSubfolder(QueryPath);

        public IList<IEntity> Configurations() => LoadEntitiesFromSubfolder(ConfigurationPath);

        private IList<IEntity> LoadEntitiesFromSubfolder(string path)
        {
            // #1. check that folder exists
            if (!CheckPathExists(Path) || !CheckPathExists(path))
                return new List<IEntity>();

            // #2 find all content-type files in folder
            var jsons = Directory.GetFiles(path, "*" + ImpExpConstants.Extension(ImpExpConstants.Files.json))
                .OrderBy(f => f)
                .ToArray();

            // #3.1 WIP - Allow relationships between loaded items
            var entitiesForRelationships = new List<IEntity>();
            var relationshipsSource = new DirectEntitiesSource(entitiesForRelationships);


            // #3.2 load entity-items from folder
            var jsonSerializer = Serializer;
            var entities = jsons
                .Select(json => LoadAndBuildEntity(jsonSerializer, json, relationshipsSource))
                .Where(entity => entity != null)
                .ToList();

            // #3.3 Put all found entities into the source
            entitiesForRelationships.AddRange(entities);
            
            return entities;
        }

        #endregion


        #region ContentType

        public IList<IContentType> ContentTypes() => ContentTypes(AppId, null);

        /// <inheritdoc />
        /// <param name="appId">this is not used ATM - just for interface compatibility, must always be 0</param>
        /// <param name="source">this is not used ATM - just for interface compatibility</param>
        /// <returns></returns>
        public IList<IContentType> ContentTypes(int appId, IHasMetadataSource source)
        {
            // v11.01 experimental - maybe disable this, as now we're loading from the app folder so we have an AppId
            if (appId != Constants.PresetAppId) throw new ArgumentOutOfRangeException(nameof(appId), appId, "appid should only be 0 for now");

            // #1. check that folder exists
            var pathCt = ContentTypePath;
            if (!CheckPathExists(Path) || !CheckPathExists(pathCt))
                return new List<IContentType>();

            // #2 find all content-type files in folder
            var jsons = Directory.GetFiles(pathCt, "*" + ImpExpConstants.Extension(ImpExpConstants.Files.json)).OrderBy(f => f);

            // #3 load content-types from folder
            var cts = jsons.Select(json => LoadAndBuildCt(Serializer, json)).Where(ct => ct != null).ToList();

            return cts;
        }

        private string ContentTypePath => Path + ContentTypeFolder;

        /// <summary>
        /// Try to load a content-type file, but if anything fails, just return a null
        /// </summary>
        /// <param name="ser"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private IContentType LoadAndBuildCt(JsonSerializer ser, string path)
        {
            Log.Add("Loading " + path);
            var infoIfError = "couldn't read type-file";
            try
            {
                var json = System.IO.File.ReadAllText(path);

                infoIfError = "couldn't deserialize string";
                var ct = ser.DeserializeContentType(json);

                infoIfError = "couldn't set source/parent";
                (ct as ContentType).SetSourceAndParent(RepoType, Constants.PresetContentTypeFakeParent, path);
                return ct;
            }
            catch (IOException e)
            {
                Log.Add("Failed loading type - couldn't import type-file, IO exception: " + e);
                return null;
            }
            catch (Exception e)
            {
                Log.Add($"Failed loading type - {infoIfError}, exception '" + e.GetType().FullName + "':" + e.Message);
                return null;
            }
        }
        #endregion


        #region todo someday items
        // 2020-07-31 2dm not used
        //private string ItemPath => Path + ItemFolder;


        /// <summary>
        /// Try to load an entity (for example a query-definition)
        /// If anything fails, just return a null
        /// </summary>
        /// <param name="ser"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private IEntity LoadAndBuildEntity(JsonSerializer ser, string path, IEntitiesSource relationshipSource = null)
        {
            Log.Add("Loading " + path);
            try
            {
                var json = System.IO.File.ReadAllText(path);
                var ct = ser.DeserializeWithRelsWip(json, allowDynamic: true, skipUnknownType: false, relationshipSource);
                return ct;
            }
            catch (IOException e)
            {
                Log.Exception(e);
                Log.Add($"Failed loading type - couldn't read file because of {e}");
                return null;
            }
            catch (Exception e)
            {
                Log.Exception(e);
                Log.Add($"Failed loading type - couldn't deserialize or unknown reason: {e}");
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
            var wrapLog = Log.Call<bool>("path: check exists '" + path + "'");
            if (Directory.Exists(path)) return wrapLog("ok", true);
            if (!IgnoreMissingStuff)
                throw new DirectoryNotFoundException("directory '" + path + "' not found, and couldn't ignore");
            Log.Add("path: doesn't exist, but ignore");
            return wrapLog("not found", false);
        }

    }
}
