using System;
using System.Collections.Generic;
using ToSic.Eav.Interfaces;

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
            bool isPublished, IEnumerable<EntityRelationshipItem> allRelationships, 
            IDeferredEntitiesList source,
            DateTime modified, string owner, int version)
        {
            var e = EntityWithAllIdsAndType(appId, entityGuid, entityId, repositoryId,
                type, isPublished, modified, owner, version);

            e.MetadataFor = metadataFor;
            e.Attributes = new Dictionary<string, IAttribute>(StringComparer.OrdinalIgnoreCase);

            //moved to RelationshipManager
            //if (allRelationships == null)
            //    allRelationships = new List<EntityRelationshipItem>();
            e.Relationships = new RelationshipManager(e, allRelationships);

            e.DeferredLookupData = source;

            return e;
        }

        private static Entity EntityWithAllIdsAndType(int appId, Guid entityGuid, int entityId, 
            int repositoryId, IContentType type, bool isPublished, 
            DateTime modified, string owner, int version) 
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
            Owner = owner
        };

        /// <summary>
        /// Create a new Entity based on an Entity and Attributes
        /// Used in the Attribute-Filter, which generates a new entity with less properties
        /// </summary>
        public static Entity FullClone(IEntity entity, Dictionary<string, IAttribute> attributes, 
            IEnumerable<EntityRelationshipItem> allRelationships)
        {
            var e = EntityWithAllIdsAndType(entity.AppId, entity.EntityGuid, entity.EntityId, entity.RepositoryId, entity.Type, 
                entity.IsPublished, entity.Modified, entity.Owner, entity.Version);
            e.TitleFieldName = entity.Title?.Name;
            e.Attributes = attributes;
            e.Relationships = new RelationshipManager(e, allRelationships);

            e.MetadataFor = new MetadataFor(entity.MetadataFor);

            e.DeferredLookupData = (entity as Entity)?.DeferredLookupData;
            return e;
        }

    }
}
