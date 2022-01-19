using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;

namespace ToSic.Eav.Persistence.File
{
    public partial class Runtime
    {
        public List<IContentType> LoadGlobalContentTypes(int typeIdSeed)
        {
            Log.Add("loading types");

            // 3 - return content types
            var types = new List<IContentType>();
            Loaders.ForEach(l =>
            {
                l.IdSeed = typeIdSeed;
                types.AddRange(l.ContentTypes());
                typeIdSeed = (l.IdSeed / Global.GlobalContentTypeSourceSkip + 1) * Global.GlobalContentTypeSourceSkip;
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
                    .ToDictionary(t => t.NameId, t => t);

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
            var typesGrouped = types.GroupBy(t => t.NameId).ToList();

            foreach (var badGroups in typesGrouped.Where(g => g.Count() > 1))
            {
                Log.Add("Warning: This type exists more than once - possibly defined in more plugins: " +
                     $"'{badGroups.First().NameId}' / '{badGroups.First().Name}'");
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
                    typeDic.TryGetValue(entity.Type.NameId, out var realType);
                    if (realType == null)
                    {
                        Log.Add("TypeUnchanged:" + entity.Type.NameId);
                        continue;
                    }
                    changeCount++;
                    Log.Add($"TypeChange:{entity.Type.NameId} - {realType.Name}");
                    (entity as Entity).UpdateType(realType, true);
                }

            return wrapLog($"{changeCount}", changeCount);
        }
    }
}
