using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Apps;
using ToSic.Eav.Metadata;

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
        public EntityBuilder(AttributeBuilder attributeBuilder) => Attribute = attributeBuilder;
        public AttributeBuilder Attribute { get; }

        public Entity Create(
            int appId,
            IContentType contentType,
            string noParamOrder = Parameters.Protector,
            IImmutableDictionary<string, IAttribute> attributes = default,
            int entityId = default,
            int repositoryId = Constants.NullId,
            Guid guid = default,
            string titleField = default,
            DateTime? created = default, DateTime? modified = default,
            string owner = default,
            int version = default,
            bool isPublished = true,
            ITarget metadataFor = default,
            EntityPartsBuilder partsBuilder = default,
            // Publishing instructions
            bool placeDraftInBranch = default,
            int publishedId = default
            )
        {
            Parameters.ProtectAgainstMissingParameterNames(noParamOrder);

            // If repositoryId isn't known set it it to EntityId
            repositoryId = repositoryId == Constants.NullId ? entityId : repositoryId;
            version = version == default ? 1 : version;

            // Prepare the Parts-builder in case it wasn't provided
            partsBuilder = partsBuilder ?? new EntityPartsBuilder(
                e => new RelationshipManager(e, null, null)
            );

            return new Entity(appId, entityId, repositoryId: repositoryId,
                partsBuilder: partsBuilder, 
                contentType: contentType,
                rawValues: null,
                values: attributes ?? Attribute.Empty(),
                guid: guid,
                titleFieldName: titleField,
                created: created,
                modified: modified,
                owner: owner,
                version: version,
                isPublished: isPublished,
                metadataFor: metadataFor,
                placeDraftInBranch: placeDraftInBranch,
                publishedId: publishedId);
        }
        

        /// <summary>
        /// Create a new Entity from a data store (usually SQL backend)
        /// </summary>
        public Entity EntityFromRepository(
            int appId,
            Guid entityGuid,
            int entityId, 
            int repositoryId, 
            ITarget metadataFor, 
            IContentType type, 
            bool isPublished, 
            AppState source,
            DateTime created,
            DateTime modified,
            string owner,
            int version,
            string titleField = default,
            IImmutableDictionary<string, IAttribute> attributes = default,
            List<IEntity> metadataItems = default
            )
        {
            var partsBuilder = new EntityPartsBuilder(
                entity => new RelationshipManager(entity, source, null),
                getMetadataOf: metadataItems != default
                    ? EntityPartsBuilder.CreateMetadataOfItems(metadataItems)
                    : EntityPartsBuilder.CreateMetadataOfAppSources(source)
            );

            return Create(
                appId: appId,
                attributes: attributes,
                guid: entityGuid, entityId: entityId, repositoryId: repositoryId,
                contentType: type, titleField: titleField,
                created: created, modified: modified,
                owner: owner, version: version, isPublished: isPublished,
                metadataFor: metadataFor,
                partsBuilder: partsBuilder
            );
        }

        /// <summary>
        /// Create an empty entity of a specific type.
        /// Usually used in edit scenarios, where the presentation doesn't exist yet
        /// </summary>
        public Entity EmptyOfType(int appId, Guid entityGuid, int entityId, IContentType type) =>
            Create(appId: appId,
                entityId: entityId,
                guid: entityGuid,
                contentType: type,
                attributes: Attribute.Create(type, null),
                created: DateTime.MinValue,
                modified: DateTime.Now, 
                owner: "");

        /// <summary>
        /// Create a new Entity based on an Entity and Attributes
        /// Used in the Attribute-Filter, which generates a new entity with less properties
        /// </summary>
        public IEntity Clone(
            IEntity original,
            string noParamOrder = Parameters.Protector,
            int? appId = default,
            IImmutableDictionary<string, IAttribute> attributes = default,
            int? id = default,
            int? repositoryId = default,
            Guid? guid = default,
            IContentType type = default,
            bool? isPublished = default,
            DateTime? created = default,
            DateTime? modified = default,
            string owner = default,
            int? version = default,
            ITarget target = default,

            // publishing Instructions
            bool? placeDraftInBranch = default,
            int? publishedId = default
            )
        {
            var originalEntity = original as Entity;

            var entityPartsBuilder = new EntityPartsBuilder(
                ent => new RelationshipManager(ent, originalEntity?.Relationships as RelationshipManager),
                getMetadataOf: (id == default && guid == default)
                    // If identifiers don't change, it will provide the identical metadata
                    ? EntityPartsBuilder.ReUseMetadataFunc<Guid>(original.Metadata)
                    // If they do change, we need to create a derived clone
                    : EntityPartsBuilder.CloneMetadataFunc<Guid>(original.Metadata)
            );

            attributes = attributes ?? originalEntity.Attributes;

            var e = Create(
                appId: appId ?? original.AppId,
                attributes: attributes, 
                entityId: id ?? original.EntityId,
                repositoryId: repositoryId ?? original.RepositoryId,
                guid: guid ?? original.EntityGuid,
                contentType: type ?? original.Type,
                titleField: originalEntity.TitleFieldName, 
                isPublished: isPublished ?? original.IsPublished,
                created: created ?? original.Created,
                modified: modified ?? original.Modified,
                owner: owner ?? original.Owner,
                version: version ?? original.Version,
                metadataFor: target ?? new Target(original.MetadataFor),

                placeDraftInBranch: placeDraftInBranch ?? originalEntity?.PlaceDraftInBranch ?? default,
                publishedId: publishedId ?? originalEntity?.PublishedEntityId ?? default,
                
                partsBuilder: entityPartsBuilder
            );

            return e;
        }

    }
}
