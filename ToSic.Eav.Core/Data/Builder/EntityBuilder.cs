using System;
using System.Collections.Generic;
using ToSic.Eav.App;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.Builder
{
    /// <summary>
    /// This is a helper environment to build entities based on different needs
    /// It's basically different kinds of constructors, just to keep the primary 
    /// Entity object lean and clear
    /// </summary>
    public static class EntityBuilder
    {
        /// <summary>
        /// Create a new Entity from a data store (usually SQL backend)
        /// </summary>
        public static Entity EntityFromRepository(int appId, Guid entityGuid, int entityId, 
            int repositoryId, IMetadataFor metadataFor, IContentType type, 
            bool isPublished, 
            AppDataPackage source,
            DateTime modified, string owner, int version)
        {
            var e = EntityWithAllIdsAndType(appId, entityGuid, entityId, repositoryId,
                type, isPublished, modified, owner, version);

            e.MetadataFor = metadataFor;

            e.Relationships = new RelationshipManager(e, source, null);

            e.DeferredLookupData = source;

            return e;
        }

        public static Entity EntityWithAttributes(int appId, Guid entityGuid, int entityId,
            int repositoryId, IContentType type, bool isPublished = true,
            DateTime? modified = null, string owner = "", int version = 1)
        {
            var ent = EntityWithAllIdsAndType(appId, entityGuid, entityId, repositoryId, 
                type, isPublished, modified ?? DateTime.Now, owner, version);

            ent.MetadataFor = new MetadataFor();

            var titleAttrib = ent.GenerateAttributesOfContentType(type);
            if (titleAttrib != null)
                ent.SetTitleField(titleAttrib.Name);
            return ent;
        }

        private static Entity EntityWithAllIdsAndType(int appId, Guid entityGuid, int entityId,
            int repositoryId, IContentType type, bool isPublished,
            DateTime modified, string owner, int version, 
            Dictionary<string, IAttribute> attribs = null)
            => new Entity
            {
                AppId = appId,
                EntityId = entityId,
                Version = version,
                EntityGuid = entityGuid,
                Type = type,
                IsPublished = isPublished,
                RepositoryId = repositoryId,
                Modified = modified,
                Owner = owner,
                Attributes = attribs ?? new Dictionary<string, IAttribute>(StringComparer.OrdinalIgnoreCase)
            };

        /// <summary>
        /// Create a new Entity based on an Entity and Attributes
        /// Used in the Attribute-Filter, which generates a new entity with less properties
        /// </summary>
        public static Entity FullClone(IEntity entity, Dictionary<string, IAttribute> attributes, 
            IEnumerable<EntityRelationshipItem> allRelationships)
        {
            var e = EntityWithAllIdsAndType(entity.AppId, entity.EntityGuid, entity.EntityId, entity.RepositoryId, entity.Type, 
                entity.IsPublished, entity.Modified, entity.Owner, entity.Version, attributes);
            e.TitleFieldName = entity.Title?.Name;
            var lookupApp = (entity as Entity)?.DeferredLookupData as AppDataPackage;
            e.Relationships = new RelationshipManager(e, lookupApp, allRelationships);

            e.MetadataFor = new MetadataFor(entity.MetadataFor);

            e.DeferredLookupData = lookupApp;
            return e;
        }


        public static IAttribute GenerateAttributesOfContentType(this IEntity newEntity, IContentType contentType)
        {
            IAttribute titleAttrib = null;
            foreach (var definition in contentType.Attributes)
            {
                var entityAttribute = ((AttributeDefinition)definition).CreateAttribute();
                newEntity.Attributes.Add(entityAttribute.Name, entityAttribute);
                if (definition.IsTitle)
                    titleAttrib = entityAttribute;
            }
            return titleAttrib;
        }
    }
}
