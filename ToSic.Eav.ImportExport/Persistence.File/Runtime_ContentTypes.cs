using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Persistence.File
{
    public partial class Runtime
    {
        public List<IContentType> LoadGlobalContentTypes() => Log.Func(() =>
        {
            // Set TypeID seed for loader so each loaded type has a unique ID
            var loaderIndex = 1;
            Loaders.ForEach(ldr => ldr.TypeIdSeed = FsDataConstants.GlobalContentTypeMin + FsDataConstants.GlobalContentTypeSourceSkip * loaderIndex++);

            // 3 - return content types
            var types = Loaders.SelectMany(ldr => ldr.ContentTypes()).ToList();

            types = SetInternalTypes(types);

            return (types, $"found {types.Count} types");
        });

        private List<IContentType> SetInternalTypes(List<IContentType> types)
        {
            var wrapLog = Log.Fn<List<IContentType>>(timer: true);
            var changeCount = 0;
            try
            {
                var typeDic = EliminateDuplicateTypes(types)
                    .ToDictionary(t => t.NameId, t => t);

                var entitiesToRetype = types.SelectMany(t => t.Metadata).ToList();
                Log.A($"Metadata found to retype: {entitiesToRetype.Count}");
                changeCount += UpdateTypes("ContentType Metadata", entitiesToRetype, typeDic);

                entitiesToRetype = types.SelectMany(t => t.Attributes.SelectMany(a => a.Metadata)).ToList();
                changeCount += UpdateTypes("Attribute Metadata", entitiesToRetype, typeDic);
            }
            catch (Exception ex)
            {
                Log.A("Error adding types");
                Log.Ex(ex);
            }

            return wrapLog.Return(types, $"{changeCount}");
        }
        
        private IEnumerable<IContentType> EliminateDuplicateTypes(List<IContentType> types)
        {
            var wrapLog = Log.Fn<IEnumerable<IContentType>>();
            // In rare cases there can be a mistake and the same type may be duplicate!
            var typesGrouped = types.GroupBy(t => t.NameId).ToList();

            foreach (var badGroups in typesGrouped.Where(g => g.Count() > 1))
            {
                Log.A("Warning: This type exists more than once - possibly defined in more plugins: " +
                     $"'{badGroups.First().NameId}' / '{badGroups.First().Name}'");
                foreach (var bad in badGroups)
                    Log.A($"Source: {bad.RepositoryAddress}");
            }

            var typesUngrouped = typesGrouped.Select(g => g.First());
            return wrapLog.Return(typesUngrouped);
        }

        private int UpdateTypes(string name, IEnumerable<IEntity> entitiesToRetype, IDictionary<string, IContentType> typeDic)
        {
            var wrapLog = Log.Fn<int>(name, timer: true);
            var changeCount = 0;
            foreach (var entity in entitiesToRetype)
                if (entity.Type.IsDynamic)
                {
                    typeDic.TryGetValue(entity.Type.NameId, out var realType);
                    if (realType == null)
                    {
                        Log.A("TypeUnchanged:" + entity.Type.NameId);
                        continue;
                    }
                    changeCount++;
                    Log.A($"TypeChange:{entity.Type.NameId} - {realType.Name}");
                    (entity as Entity).UpdateType(realType, true);
                }

            return wrapLog.ReturnAndLog(changeCount);
        }
    }
}
