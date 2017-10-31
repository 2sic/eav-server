﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.App;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Types;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File
{
    public class FileSystemLoader: HasLog, IRepositoryLoader
    {
        private const string ContentTypeFolder = "ContentTypes\\";

        public FileSystemLoader(string path, bool ignoreMissing, Log parentLog): base("FSL.Loadr", parentLog, "init")
        {
            Path = path + (path.EndsWith("\\") ? "" : "\\");
            Path = path;
            IgnoreMissingStuff = ignoreMissing;
        }

        private string Path { get; }

        private bool IgnoreMissingStuff { get; }

        public IList<IContentType> ContentTypes(int appId, IDeferredEntitiesList source)
        {
            if(appId != 0)
                throw new ArgumentOutOfRangeException(nameof(appId), appId, "appid should only be 0 for now");



            #region #1. check that folder exists

            if (!CheckPathExists(Path)) return null;
            var pathCt = Path + ContentTypeFolder;
            if (!CheckPathExists(pathCt)) return null;

            #endregion


            #region #2 find all content-type files in folder

            var jsons = Directory.GetFiles(pathCt, "*.json").OrderBy(f => f);

            #endregion

            #region #3 load content-types from folder

            var ser = new JsonSerializer();
            ser.Initialize(0, Global.SystemContentTypes().Values, null);
            ser.AssumeUnknownTypesAreDynamic = true;

            var cts = jsons.Select(json => LoadAndBuildCt(ser, json)).Where(ct => ct != null).ToList();

            #endregion

            return cts;
        }


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
                return ser.DeserializeContentType(json);
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

        private bool CheckPathExists(string path)
        {
            Log.Add("path: check exists '" + path + "'");
            if (Directory.Exists(path)) return true;
            if (!IgnoreMissingStuff)
                throw new DirectoryNotFoundException("directory '" + path + "' not found, and couldn't ignore");
            Log.Add("path: doesn't exist, but ignore");
            return false;
        }

        public AppDataPackage AppPackage(int appId, int[] entityIds = null, bool entitiesOnly = false, Log parentLog = null)
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, Zone> Zones()
        {
            throw new NotImplementedException();
        }
    }
}
