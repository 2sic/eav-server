using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Data.ContentTypes;
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

            var types = new List<IContentType>();
            var delayedContentTypeProvider = new ContentTypeProvider();

            var newTypes = Loaders.SelectMany(ldr =>
            {
                ldr.Serializer.ContentTypeProvider = delayedContentTypeProvider;
                var result = ldr.ContentTypes();
                ldr.Serializer.ContentTypeProvider = null;
                return result;
            }).ToList();
            var typesDeduplicated = EliminateDuplicateTypes(newTypes);
            types.AddRange(typesDeduplicated);
            delayedContentTypeProvider.Source.AddRange(typesDeduplicated);

            types = SetTypesOfContentTypeParts(types);

            return (types, $"found {types.Count} types");
        });

        
        private List<IContentType> SetTypesOfContentTypeParts(List<IContentType> typesDistinct) => Log.Func(timer: true, func: l =>
        {
            var changeCount = 0;
            try
            {
                var typeDic = typesDistinct.ToDictionary(t => t.NameId, t => t);

                var entitiesToRetype = typesDistinct.SelectMany(t => t.Metadata).ToList();
                l.A($"Metadata found to retype: {entitiesToRetype.Count}");
                var temp = UpdateTypes("ContentType Metadata", entitiesToRetype, typeDic);
                changeCount += temp.Count;

                entitiesToRetype = typesDistinct.SelectMany(t => t.Attributes.SelectMany(a => a.Metadata)).ToList();
                temp = UpdateTypes("Attribute Metadata", entitiesToRetype, typeDic);
                changeCount += temp.Count;
            }
            catch (Exception ex)
            {
                l.A("Error adding types");
                l.Ex(ex);
            }

            return (types: typesDistinct, $"{changeCount}");
        });

        private List<IContentType> EliminateDuplicateTypes(List<IContentType> types) => Log.Func(l =>
        {
            // In rare cases there can be a mistake and the same type may be duplicate!
            var typesGrouped = types.GroupBy(t => t.NameId).ToList();

            foreach (var badGroups in typesGrouped.Where(g => g.Count() > 1))
            {
                l.A("Warning: This type exists more than once - possibly defined in more plugins: " +
                      $"'{badGroups.First().NameId}' / '{badGroups.First().Name}'");
                foreach (var bad in badGroups)
                    l.A($"Source: {bad.RepositoryAddress}");
            }

            var typesUngrouped = typesGrouped
                .Select(g => g.First())
                .ToList();
            return typesUngrouped;
        });

        private (List<IEntity> Changed, int Count) UpdateTypes(string name, List<IEntity> entitiesToRetype, IDictionary<string, IContentType> typeDic
        ) => Log.Func($"For {name}", timer: true, func: l =>
        {
            var changeCount = 0;
            var sameChanged = entitiesToRetype
                .Select(entity =>
                {
                    if (!entity.Type.IsDynamic) return entity;

                    typeDic.TryGetValue(entity.Type.NameId, out var realType);
                    if (realType == null)
                    {
                        l.A("TypeUnchanged:" + entity.Type.NameId);
                        return entity;
                    }

                    changeCount++;
                    l.A($"TypeChange:{entity.Type.NameId} - {realType.Name}");
                    if (entity.Type is ContentTypeWrapper wrapper)
                        wrapper.Reset();
                    //(entity as Entity).UpdateType(realType);
                    return entity;
                })
                .ToList();

            return (entitiesToRetype, changeCount);
        });
    }
}
