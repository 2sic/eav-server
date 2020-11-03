using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repositories;
using ToSic.Eav.Run;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.ImportExport.Persistence.File
{
    public class Runtime : HasLog, IRuntime
    {
        public const string GroupQuery = "query";
        public const string GroupConfiguration = "configuration";

        #region Constructor and DI

        private readonly IServiceProvider _serviceProvider;
        public Runtime(IServiceProvider sc) : base("Eav.Rntime") => _serviceProvider = sc;

        public IRuntime Init(ILog parent)
        {
            Log.LinkTo(parent);
            return this;
        }

        #endregion

        // 1 - find the current path to the .data folder
        public List<string> Paths
        {
            get
            {
                if (_paths != null) return _paths;
                Log.Add("start building path-list");

                _paths = new List<string>();
                // find all RepositoryInfoOfFolder and let them tell us what paths to use
                var types = AssemblyHandling.FindInherited(typeof(RepositoryInfoOfFolder), Log).ToList();
                Log.Add($"found {types.Count} Path providers");

                foreach (var typ in types)
                    try
                    {
                        Log.Add($"adding {typ.FullName}");
                        var instance = (RepositoryInfoOfFolder) ActivatorUtilities.CreateInstance(_serviceProvider, typ, new object[0]);
                        var paths = instance.RootPaths;
                        if (paths != null)
                            _paths.AddRange(paths);
                    }
                    catch(Exception e)
                    {
                        Log.Add($"ran into a problem with one of the path providers: {typ?.FullName} - will skip. Message: {e.Message}");
                        /* ignore */
                    }
                Log.Add($"done, found {_paths.Count} paths");
                Log.Add(() => string.Join(",", _paths));
                return _paths;
            }
        }

        private List<string> _paths;

        public RepositoryTypes Source { get; } = RepositoryTypes.Folder;

        public IEnumerable<IContentType> LoadGlobalContentTypes()
        {
            Log.Add("loading types");

            // 3 - return content types
            var types = new List<IContentType>();
            Loaders.ForEach(l => types.AddRange(l.ContentTypes()));
            Log.Add($"found {types.Count} types");
            return types;
        }



        internal List<FileSystemLoader> Loaders => _loader ?? (_loader = Paths.Select(path => new FileSystemLoader(path, Source, true, null, Log)).ToList());
        private List<FileSystemLoader> _loader;


        public IEnumerable<Data.IEntity> LoadGlobalItems(string groupIdentifier)
        {
            Log.Add($"loading items for {groupIdentifier}");

            if(groupIdentifier != GroupQuery && groupIdentifier != GroupConfiguration)
                throw new ArgumentOutOfRangeException(nameof(groupIdentifier), "atm we can only load items of type 'query'/'configuration'");

            var doQuery = groupIdentifier == GroupQuery;

            // 3 - return content types
            var entities = new List<IEntity>();
            Loaders.ForEach(l => entities.AddRange(doQuery ? l.Queries() : l.Configurations()));
            Log.Add($"found {entities.Count} items of type {groupIdentifier}");
            return entities;
        }


    }
}