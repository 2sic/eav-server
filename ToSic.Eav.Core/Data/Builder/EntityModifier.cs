using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data.Builder
{
    /// <summary>
    /// Helper commands to build an entity
    /// Should only be used by internal system data handlers - not for "public" use
    /// </summary>
    public static class EntityModifier
    {
        public static void SetGuid(this Entity entity, Guid newGuid) => entity.EntityGuid = newGuid;

        public static void SetTitleField(this Entity entity, string name) => entity.TitleFieldName = name;

        public static void SetMetadata(this Entity entity, MetadataFor meta) => entity.MetadataFor = meta;

        public static void SetType(this Entity entity, IContentType contentType) => entity.Type = contentType;

        public static int? GetPublishedIdForSaving(this Entity entity) => entity.PublishedEntity?.EntityId ?? entity.PublishedEntityId;
        public static int? ChangeIdForSaving(this IEntity entity, int newId) => ((EntityLight) entity).EntityId = newId;

        public static int? VersionIncrease(this Entity entity) => entity.Version++;

        public static void SetPublishedIdForSaving(this Entity entity, int? publishedId) => entity.PublishedEntityId = publishedId;

        public static List<ILanguage> GetUsedLanguages(this IEntity entity) => entity.Attributes?.Values
            .SelectMany(v => v.Values)
            .SelectMany(vl => vl.Languages)
            .GroupBy(l => l.Key)
            .Select(g => g.First())
            .ToList() ?? new List<ILanguage>();

    }
}
