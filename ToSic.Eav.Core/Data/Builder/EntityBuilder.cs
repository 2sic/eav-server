using System;
using System.Collections.Generic;
using ToSic.Eav.Metadata;
using AppState = ToSic.Eav.Apps.AppState;

namespace ToSic.Eav.Data.Builder
{
    /// <summary>
    /// This is a helper environment to build entities based on different needs
    /// It's basically different kinds of constructors, just to keep the primary 
    /// Entity object lean and clean
    /// </summary>
    public class EntityBuilder
    {
        /// <summary>
        /// Constructor - should never be called as it should be used with DI
        /// </summary>
        public EntityBuilder() { }

        /// <summary>
        /// Create a new Entity from a data store (usually SQL backend)
        /// </summary>
        public Entity EntityFromRepository(int appId, Guid entityGuid, int entityId, 
            int repositoryId, ITarget metadataFor, IContentType type, 
            bool isPublished, 
            AppState source,
            DateTime created,
            DateTime modified, string owner, int version)
        {
            var e = EntityWithAllIdsAndType(appId, entityGuid, entityId, repositoryId,
                type, isPublished, created, modified, owner, version);

            e.MetadataFor = metadataFor;

            e.Relationships = new RelationshipManager(e, source, null);

            e.DeferredLookupData = source;

            return e;
        }

        [Obsolete("Unclear where this is used or actually needed - will probably remove soon")]
        public Entity EntityTemplate(int appId, Guid entityGuid, int entityId,
            int repositoryId, IContentType type)
        {
            var ent = EntityWithAllIdsAndType(appId, entityGuid, entityId, repositoryId, 
                type, true, DateTime.MinValue, DateTime.Now, "", 1);

            ent.MetadataFor = new Target();

            var titleAttrib = GenerateAttributesOfContentType(ent, type);
            if (titleAttrib != null)
                ent.SetTitleField(titleAttrib.Name);
            return ent;
        }

        private static Entity EntityWithAllIdsAndType(int appId, Guid entityGuid, int entityId,
            int repositoryId, IContentType type, bool isPublished,
            DateTime created,
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
                Created = created,
                Modified = modified,
                Owner = owner,
                Attributes = attribs
            };

        /// <summary>
        /// Create a new Entity based on an Entity and Attributes
        /// Used in the Attribute-Filter, which generates a new entity with less properties
        /// </summary>
        public Entity FullClone(IEntity entity, 
            Dictionary<string, IAttribute> attributes, 
            IEnumerable<EntityRelationship> allRelationships,
            IContentType newType = null)
        {
            var targetType = newType ?? entity.Type;

            var e = EntityWithAllIdsAndType(entity.AppId, entity.EntityGuid, entity.EntityId, entity.RepositoryId, targetType, 
                entity.IsPublished, entity.Created, entity.Modified, entity.Owner, entity.Version, attributes);
            e.TitleFieldName = entity.Title?.Name;
            var lookupApp = (entity as Entity)?.DeferredLookupData as AppState;
            e.Relationships = new RelationshipManager(e, lookupApp, allRelationships);

            e.MetadataFor = new Metadata.Target(entity.MetadataFor);

            e.DeferredLookupData = lookupApp;
            return e;
        }

        // TODO: MOVE TO AttributeBuilder
        public IAttribute GenerateAttributesOfContentType(IEntity newEntity, IContentType contentType)
        {
            IAttribute titleAttrib = null;
            foreach (var definition in contentType.Attributes)
            {
                var entityAttribute = ((ContentTypeAttribute)definition).CreateAttribute();
                newEntity.Attributes.Add(entityAttribute.Name, entityAttribute);
                if (definition.IsTitle)
                    titleAttrib = entityAttribute;
            }
            return titleAttrib;
        }
    }
}
