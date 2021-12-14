using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
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
        #region Constructor and DI

        public Runtime(IServiceProvider sp) : base("Eav.Rntime") => _serviceProvider = sp;
        private readonly IServiceProvider _serviceProvider;

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
                var types = AssemblyHandling.FindInherited(typeof(FolderBasedRepository), Log).ToList();
                Log.Add($"found {types.Count} Path providers");

                foreach (var typ in types)
                    try
                    {
                        Log.Add($"adding {typ.FullName}");
                        var instance = (FolderBasedRepository) ActivatorUtilities.CreateInstance(_serviceProvider, typ, Array.Empty<object>());
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

        public List<IContentType> LoadGlobalContentTypes(int typeIdSeed)
        {
            Log.Add("loading types");

            // 3 - return content types
            var types = new List<IContentType>();
            Loaders.ForEach(l =>
            {
                l.IdSeed = typeIdSeed;
                types.AddRange(l.ContentTypes());
                typeIdSeed = (l.IdSeed / Global.GlobalContentTypeSourceSkip + 1 ) * Global.GlobalContentTypeSourceSkip;
            });

            types = SetInternalTypes(types);


            Log.Add($"found {types.Count} types");
            return types;
        }

        private List<IContentType> SetInternalTypes(List<IContentType> types)
        {
            var wrapLog = Log.Call<List<IContentType>>(useTimer: true);
            var changeCount = 0;
            try
            {
                var typeDic = EliminateDuplicateTypes(types)
                    .ToDictionary(t => t.StaticName, t => t);

                var entitiesToRetype = types.SelectMany(t => t.Metadata).ToList();
                Log.Add($"Metadata found to retype: {entitiesToRetype.Count}");
                changeCount += UpdateTypes("ContentType Metadata", entitiesToRetype, typeDic);

                entitiesToRetype = types.SelectMany(t => t.Attributes.SelectMany(a => a.Metadata)).ToList();
                changeCount += UpdateTypes("Attribute Metadata", entitiesToRetype, typeDic);
            }
            catch (Exception ex)
            {
                Log.Add("Error adding types");
                Log.Exception(ex);
            }

            return wrapLog($"{changeCount}", types);
        }

        private IEnumerable<IContentType> EliminateDuplicateTypes(List<IContentType> types)
        {
            var wrapLog = Log.Call<IEnumerable<IContentType>>();
            // In rare cases there can be a mistake and the same type may be duplicate!
            var typesGrouped = types.GroupBy(t => t.StaticName).ToList();

            foreach (var badGroups in typesGrouped.Where(g => g.Count() > 1))
            {
                Log.Add("Warning: This type exists more than once - possibly defined in more plugins: " +
                     $"'{badGroups.First().StaticName}' / '{badGroups.First().Name}'");
                foreach (var bad in badGroups) 
                    Log.Add($"Source: {bad.RepositoryAddress}");
            }

            var typesUngrouped = typesGrouped.Select(g => g.First());
            return wrapLog(null, typesUngrouped);
        }

        private int UpdateTypes(string name, IEnumerable<IEntity> entitiesToRetype, IDictionary<string, IContentType> typeDic)
        {
            var wrapLog = Log.Call<int>(name, useTimer: true);
            var changeCount = 0;
            foreach (var entity in entitiesToRetype)
                if (entity.Type.IsDynamic)
                {
                    typeDic.TryGetValue(entity.Type.StaticName, out var realType); // types.FirstOrDefault(t => t.Is(entity.Type.StaticName));
                    if (realType == null)
                    {
                        Log.Add("TypeUnchanged:" + entity.Type.StaticName);
                        continue;
                    }
                    changeCount++;
                    Log.Add($"TypeChange:{entity.Type.StaticName} - {realType.Name}");
                    (entity as Entity).UpdateType(realType, true);
                }

            return wrapLog($"{changeCount}", changeCount);
        }



        internal List<FileSystemLoader> Loaders => _loader ?? (_loader = Paths
            .Select(path => _serviceProvider.Build<FileSystemLoader>().Init(Constants.PresetAppId, path, Source, true, null, Log)).ToList());
        private List<FileSystemLoader> _loader;


        public IEnumerable<IEntity> LoadGlobalItems(string groupIdentifier)
        {
            Log.Add($"loading items for {groupIdentifier}");

            if(groupIdentifier != Configuration.Global.GroupQuery && groupIdentifier != Configuration.Global.GroupConfiguration)
                throw new ArgumentOutOfRangeException(nameof(groupIdentifier), "atm we can only load items of type 'query'/'configuration'");

            var doQuery = groupIdentifier == Configuration.Global.GroupQuery;

            // 3 - return content types
            var entities = new List<IEntity>();
            Loaders.ForEach(l => entities.AddRange(doQuery ? l.Queries() : l.Configurations()));
            Log.Add($"found {entities.Count} items of type {groupIdentifier}");
            return entities;
        }


    }
}