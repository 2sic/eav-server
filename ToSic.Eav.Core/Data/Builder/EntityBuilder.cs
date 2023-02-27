using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Generics;
using ToSic.Eav.Metadata;
using static System.StringComparer;
// ReSharper disable ConvertToNullCoalescingCompoundAssignment

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
        public EntityBuilder(AttributeBuilder attributeBuilder) => _attributeBuilder = attributeBuilder;
        private readonly AttributeBuilder _attributeBuilder;

        public Entity Create(
            int appId,
            IContentType contentType,
            string noParamOrder = Eav.Parameters.Protector,
            Dictionary<string, object> values = default,
            Dictionary<string, IAttribute> typedValues = default,
            int entityId = default,
            int repositoryId = Constants.NullId,
            Guid guid = default,
            string titleField = default,
            DateTime? created = default, DateTime? modified = default,
            string owner = default,
            int version = default,
            bool isPublished = true,
            ITarget metadataFor = default,
            EntityPartsBuilder partsBuilder = default
            )
        {
            // if we have typed, make sure invariant
            typedValues = typedValues?.ToInvariant();

            // If typed and basic values don't exist, set Typed as new list for now WIP
            if (typedValues == null && values == null)
                typedValues = new Dictionary<string, IAttribute>(InvariantCultureIgnoreCase);

            // Typed values exist if given explicitly, OR if the values are of the desired type
            if (typedValues == null && values.All(x => x.Value is IAttribute))
                typedValues = values.ToDictionary(pair => pair.Key, pair => pair.Value as IAttribute, InvariantCultureIgnoreCase);

            // If TypedValues still don't exist
            var useLightMode = typedValues == null;
            if (typedValues == null)
                typedValues = _attributeBuilder.ConvertToIAttributeDic(values);
            else
                values = null;

            // If repositoryId isn't known set it it to EntityId
            repositoryId = repositoryId == Constants.NullId ? entityId : repositoryId;
            version = version == default ? 1 : version;

            // Prepare the Parts-builder in case it wasn't provided
            partsBuilder = partsBuilder ?? new EntityPartsBuilder(
                e => new RelationshipManager(e, null, null)
            );

            return new Entity(appId, entityId, partsBuilder: partsBuilder,  repositoryId: repositoryId, contentType: contentType,
                useLightMode: useLightMode, values: values, typedValues: typedValues,
                guid: guid, titleFieldName: titleField,
                created: created, modified: modified, owner: owner,
                version: version, isPublished: isPublished,
                metadataFor: metadataFor);
        }

        /// <summary>
        /// Create a new Entity from a data store (usually SQL backend)
        /// </summary>
        public Entity EntityFromRepository(int appId, Guid entityGuid, int entityId, 
            int repositoryId, 
            ITarget metadataFor, 
            IContentType type, 
            bool isPublished, 
            AppState source,
            DateTime created,
            DateTime modified, string owner, int version,
            string titleField = default,
            Dictionary<string, IAttribute> values = default
            )
        {
            var partsBuilder = new EntityPartsBuilder(
                entity => new RelationshipManager(entity, source, null)
            );

            var e = Create(appId: appId,
                values: null, typedValues: values,
                guid: entityGuid, entityId: entityId, repositoryId: repositoryId,
                contentType: type, titleField: titleField,
                created: created, modified: modified,
                owner: owner, version: version, isPublished: isPublished,
                metadataFor: metadataFor,
                partsBuilder: partsBuilder
            );

            e.DeferredLookupData = source;

            return e;
        }

        /// <summary>
        /// Create an empty entity of a specific type.
        /// Usually used in edit scenarios, where the presentation doesn't exist yet
        /// </summary>
        public Entity EmptyOfType(int appId, Guid entityGuid, int entityId, IContentType type)
        {
            var specs = _attributeBuilder.GenerateAttributesOfContentType(type);
            return Create(appId: appId,
                entityId: entityId, guid: entityGuid,
                contentType: type, titleField: specs.Title,
                values: null, typedValues: specs.All,
                created: DateTime.MinValue, modified: DateTime.Now, 
                owner: "");

        }

        /// <summary>
        /// Create a new Entity based on an Entity and Attributes
        /// Used in the Attribute-Filter, which generates a new entity with less properties
        /// </summary>
        public Entity Clone(IEntity entity, 
            Dictionary<string, IAttribute> newValues = default,
            IEnumerable<EntityRelationship> entityRelationshipsIfNoApp = default,
            int? newId = default,
            int? newRepoId = default,
            Guid? newGuid = default,
            IContentType newType = default
            )
        {
            var entityPartsBuilder = new EntityPartsBuilder(
                ent =>
                {
                    var lookupApp2 = (entity as Entity)?.DeferredLookupData as AppState;
                    return new RelationshipManager(ent, lookupApp2, entityRelationshipsIfNoApp);
                }
            );
            newValues = newValues ?? entity.Attributes;
            newType = newType ?? entity.Type;

            var e = Create(appId: entity.AppId,
                values: null,
                typedValues: newValues,
                entityId: newId ?? entity.EntityId,
                repositoryId: newRepoId ?? entity.RepositoryId,
                guid: newGuid ?? entity.EntityGuid,
                contentType: newType,
                titleField: entity.Title?.Name,
                isPublished: entity.IsPublished,
                created: entity.Created, modified: entity.Modified,
                owner: entity.Owner, version: entity.Version,
                metadataFor: new Target(entity.MetadataFor),
                partsBuilder: entityPartsBuilder
            );

            var lookupApp = (entity as Entity)?.DeferredLookupData as AppState;
            e.DeferredLookupData = lookupApp;
            return e;
        }


        #region Entity Pre-Save - TODO WIP 
        // 1. first make sure all the calls are here, and return the _same_ entity
        // 2. Then make sure the caller always uses the result, not the original entity
        // 3. Then enforce cloning

        public IEntity ResetIdentifiers(IEntity entity,
            string noParamOrder = Eav.Parameters.Protector,
            Guid? newGuid = default,
            int? newId = default,
            ITarget metadataFor = default,
            bool? isPublished = default,
            bool? placeDraftInBranch = default,
            int? version = default,
            int? publishedId = default)
        {
            var editable = (Entity)entity; // todo: clone
            if (newGuid != null) editable.EntityGuid = newGuid.Value;
            if (isPublished != null) editable.IsPublished = isPublished.Value;
            if (placeDraftInBranch != null) editable.PlaceDraftInBranch = placeDraftInBranch.Value;
            if (metadataFor != default) editable.MetadataFor = metadataFor;
            if (version != default) editable.Version = version.Value;
            if (newId != default) editable.EntityId = newId.Value;
            if (publishedId != default) editable.PublishedEntityId = publishedId.Value;

            if (newId != null)
            {
                editable.RepositoryId = newId.Value;//note: this was not in before, could cause side-effects
            }

            return entity;
        }

        #endregion


        // WIP - when done move elsewhere and probably rename
        public enum CloneRelationships
        {
            Unknown = 0,
            UseOriginalList = 1,
            GetFromApp = 2,
            NoRelationships,
        }
    }
}
