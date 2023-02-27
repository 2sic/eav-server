using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.Builder
{
    /// <summary>
    /// Helper commands to build an entity
    /// Should only be used by internal system data handlers - not for "public" use
    /// </summary>
    public static class EntityPreSave
    {
        //public static void SetGuid(this IEntity entity, Guid newGuid) => ((EntityLight) entity).EntityGuid = newGuid;

        //public static void SetTitleField(this Entity entity, string name) => entity.TitleFieldName = name;

        public static void SetMetadata(this Entity entity, ITarget meta) => entity.MetadataFor = meta;

        //public static void Retarget(this Entity entity, Guid newTarget)
        //    => entity.SetMetadata(new Metadata.Target(entity.MetadataFor, keyGuid: newTarget));

        public static void UpdateType(this Entity entity, IContentType newType)
        {
            if (entity.Type.Name != newType.Name && entity.Type.NameId != newType.NameId)
                throw new Exception("trying to update the type definition - but the new type is different");

            entity.Type = newType;
        }

        public static int? GetInternalPublishedIdForSaving(this Entity entity) => entity.PublishedEntity?.EntityId ?? entity.PublishedEntityId;
        
        // this is old method that probably should be replaced with new ResetEntityIdAll after testing
        public static int? ResetEntityId(this IEntity entity, int newId = 0) => ((EntityLight)entity).EntityId = newId;
        
        // this is new for bundles in v15, probably it should replace ResetEntityId everywhere but we have to test that first
        public static int? ResetEntityIdAll(this IEntity entity, int newId = 0)
        {
            ((Entity) entity).RepositoryId = newId;
            return ((EntityLight) entity).EntityId = newId;
        }

        //public static int? SetVersion(this Entity entity, int newVersion) => entity.Version = newVersion;

        //public static void SetPublishedIdForSaving(this Entity entity, int? publishedId) => entity.PublishedEntityId = publishedId;

        public static List<ILanguage> GetUsedLanguages(this IEntity entity) => entity.Attributes?.Values
            .SelectMany(v => v.Values)
            .SelectMany(vl => vl.Languages)
            .GroupBy(l => l.Key)
            .Select(g => g.First())
            .ToList() ?? DimensionBuilder.NoLanguages.ToList();

    }
}
