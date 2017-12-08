﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repositories;

namespace ToSic.Eav.ImportExport.Persistence.File
{
    public class Runtime : HasLog, IRuntime
    {
        public Runtime() : this("Eav.Rntime") {}
        public Runtime(string logName) : base(logName)
        {

        }

        // 1 - find the current path to the .data folder
        public List<string> Paths
        {
            get
            {
                if (_paths != null) return _paths;
                Log.Add("start building path-list");

                _paths = new List<string>();
                // find all RepositoryInfoOfFolder and let them tell us what paths to use
                var types = AssemblyHandling.FindInherited(typeof(RepositoryInfoOfFolder)).ToList();
                Log.Add($"found {types.Count} Path providers");

                foreach (var typ in types)
                    try
                    {
                        Log.Add($"adding {typ.FullName}");
                        var instance = (RepositoryInfoOfFolder) Activator.CreateInstance(typ);
                        var paths = instance.RootPaths;
                        if (paths != null)
                            _paths.AddRange(paths);
                    }
                    catch
                    {
                        Log.Add("ran into a problem with one of the path providers - will skip");
                        /* ignore */
                    }
                Log.Add($"done, found {_paths.Count} paths");
                Log.Add(() => string.Join(",", _paths));
                return _paths;
            }
        }

        private List<string> _paths;

        public RepositoryTypes Source { get; set; } = RepositoryTypes.Folder;

        public IEnumerable<IContentType> LoadGlobalContentTypes()
        {
            Log.Add("loading types");

            // 3 - return content types
            var types = new List<IContentType>();
            Loaders.ForEach(l => types.AddRange(l.ContentTypes()));
            Log.Add($"found {types.Count} types");
            return types;
        }



        internal List<FileSystemLoader> Loaders => _loader ?? (_loader = Paths.Select(path => new FileSystemLoader(path, Source, true, Log)).ToList());
        private List<FileSystemLoader> _loader;


        public IEnumerable<IEntity> LoadGlobalItems(string groupIdentifier)
        {
            Log.Add($"loading items for {groupIdentifier}");

            if(groupIdentifier != "query")
                throw new ArgumentOutOfRangeException(nameof(groupIdentifier), "atm we can only load items of type 'query'");

            // 3 - return content types
            var query = new List<IEntity>();
            Loaders.ForEach(l => query.AddRange(l.Queries()));
            Log.Add($"found {query.Count} queries");
            return query;
        }


    }
}