using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.App;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Repositories;
using ToSic.Eav.Types;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File
{
    public partial class FileSystemLoader: HasLog, IRepositoryLoader
    {
        private const string ContentTypeFolder = "contenttypes\\";
        private const string ItemFolder = "items\\";

        public FileSystemLoader(string path, RepositoryTypes source, bool ignoreMissing, Log parentLog): base("FSL.Loadr", parentLog, $"init with path:{path} ignore:{ignoreMissing}")
        {
            Path = path + (path.EndsWith("\\") ? "" : "\\");
            Source = source;
            IgnoreMissingStuff = ignoreMissing;
        }

        private string Path { get; }

        private bool IgnoreMissingStuff { get; }

        private RepositoryTypes Source { get; }

        private const string JsonExtension = ".json";

        public IList<IContentType> ContentTypes(int appId, IDeferredEntitiesList source)
        {
            if(appId != 0)
                throw new ArgumentOutOfRangeException(nameof(appId), appId, "appid should only be 0 for now");

            #region #1. check that folder exists

            var pathCt = ContentTypePath;
            if (!CheckPathExists(Path) || !CheckPathExists(pathCt))
                return new List<IContentType>();

            #endregion


            #region #2 find all content-type files in folder

            var jsons = Directory.GetFiles(pathCt, "*" + JsonExtension).OrderBy(f => f);

            #endregion

            #region #3 load content-types from folder


            var cts = jsons.Select(json => LoadAndBuildCt(Serializer, json)).Where(ct => ct != null).ToList();

            #endregion

            return cts;
        }

        private JsonSerializer Serializer
        {
            get
            {
                if (_ser != null) return _ser;
                _ser = new JsonSerializer();
                _ser.Initialize(0, Global.CodeContentTypes().Values, null, Log);
                _ser.AssumeUnknownTypesAreDynamic = true;
                return _ser;
            }
        }

        private JsonSerializer _ser;

        private string ContentTypePath => Path + ContentTypeFolder;

        private string ItemPath => Path + ItemFolder;

        /// <summary>
        /// Try to load a content-type file, but if anything fails, just return a null
        /// </summary>
        /// <param name="ser"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private IContentType LoadAndBuildCt(JsonSerializer ser, string path)
        {
            Log.Add("Loading " + path);
            try
            {
                var json = System.IO.File.ReadAllText(path);
                var ct = ser.DeserializeContentType(json);
                (ct as ContentType).SetSourceAndParent(Source, Constants.SystemContentTypeFakeParent, path);
                return ct;
            }
            catch (IOException e)
            {
                Log.Add("Failed loading type - couldn't read file because of " + e);
                return null;
            }
            catch
            {
                Log.Add("Failed loading type - couldn't deserialize or unknown reason");
                return null;
            }
        }

        /// <summary>
        /// Check if a path exists - if missing path is forbidden, will raise error
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool CheckPathExists(string path)
        {
            Log.Add("path: check exists '" + path + "'");
            if (Directory.Exists(path)) return true;
            if (!IgnoreMissingStuff)
                throw new DirectoryNotFoundException("directory '" + path + "' not found, and couldn't ignore");
            Log.Add("path: doesn't exist, but ignore");
            return false;
        }


        #region not implemented stuff
        public AppDataPackage AppPackage(int appId, int[] entityIds = null, bool entitiesOnly = false, Log parentLog = null)
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, Zone> Zones()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
