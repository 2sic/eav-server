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
            string noParamOrder = Parameters.Protector,
            Dictionary<string, object> rawValues = default,
            Dictionary<string, IAttribute> values = default,
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
            (values, rawValues) = PreprocessValues(values, rawValues);

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
                rawValues: rawValues, values: values,
                guid: guid, titleFieldName: titleField,
                created: created, modified: modified, owner: owner,
                version: version, isPublished: isPublished,
                metadataFor: metadataFor,
                placeDraftInBranch: placeDraftInBranch,
                publishedId: publishedId);
        }

        private (Dictionary<string, IAttribute> values, Dictionary<string, object> rawValues) PreprocessValues(Dictionary<string, IAttribute> values, Dictionary<string, object> rawValues)
        {
            // if we have typed, make sure invariant
            values = values?.ToInvariant();

            // If typed and basic values don't exist, set Typed as new list for now WIP
            if (values == null && rawValues == null)
                values = new Dictionary<string, IAttribute>(InvariantCultureIgnoreCase);

            // Typed values exist if given explicitly, OR if the values are of the desired type
            if (values == null && rawValues.All(x => x.Value is IAttribute))
                values = rawValues.ToDictionary(pair => pair.Key, pair => pair.Value as IAttribute, InvariantCultureIgnoreCase);

            // If TypedValues still don't exist
            var useLightMode = values == null;
            if (useLightMode)
                values = _attributeBuilder.ConvertToIAttributeDic(rawValues);
            else
                rawValues = null;

            return (values, rawValues);
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
            DateTime modified,
            string owner,
            int version,
            string titleField = default,
            Dictionary<string, IAttribute> values = default,
            List<IEntity> metadataItems = default,
            EntityPartsBuilder partsBuilder = default
            )
        {
            partsBuilder = partsBuilder ?? new EntityPartsBuilder(
                entity => new RelationshipManager(entity, source, null),
                getMetadataOf: metadataItems != default
                    ? EntityPartsBuilder.CreateMetadataOfItems(metadataItems)
                    : EntityPartsBuilder.CreateMetadataOfAppSources(source)
            );

            var e = Create(appId: appId,
                rawValues: null, values: values,
                guid: entityGuid, entityId: entityId, repositoryId: repositoryId,
                contentType: type, titleField: titleField,
                created: created, modified: modified,
                owner: owner, version: version, isPublished: isPublished,
                metadataFor: metadataFor,
                partsBuilder: partsBuilder
            );

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
                contentType: type,
                // titleField: specs.Title,
                rawValues: null,
                values: specs,
                created: DateTime.MinValue, modified: DateTime.Now, 
                owner: "");

        }

        /// <summary>
        /// Create a new Entity based on an Entity and Attributes
        /// Used in the Attribute-Filter, which generates a new entity with less properties
        /// </summary>
        public IEntity Clone(
            IEntity original,
            string noParamOrder = Parameters.Protector,
            int? appId = default,
            Dictionary<string, IAttribute> values = default,
            //IEnumerable<EntityRelationship> entityRelationshipsIfNoApp = default,
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

            var e = Create(
                appId: appId ?? original.AppId,
                rawValues: null,
                values: values ?? original.Attributes,
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


        #region Entity Pre-Save - TODO WIP 
        // 1. first make sure all the calls are here, and return the _same_ entity
        // 2. Then make sure the caller always uses the result, not the original entity
        // 3. Then enforce cloning

        //public IEntity ResetIdentifiers(
        //    IEntity entity,
        //    string noParamOrder = Parameters.Protector,
        //    Guid? newGuid = default,
        //    int? newId = default,
        //    ITarget metadataFor = default,
        //    bool? isPublished = default
        //    //bool? placeDraftInBranch = default,
        //    //int? version = default,
        //    //int? publishedId = default
        //    )
        //{
        //    var editable = (Entity)entity; // todo: clone
        //    if (newGuid != null) editable.EntityGuid = newGuid.Value;
        //    if (isPublished != null) editable.IsPublished = isPublished.Value;
        //    //if (placeDraftInBranch != null) editable.PlaceDraftInBranch = placeDraftInBranch.Value;
        //    if (metadataFor != default) editable.MetadataFor = metadataFor;
        //    //if (version != default) editable.Version = version.Value;
        //    if (newId != default) editable.EntityId = newId.Value;
        //    //if (publishedId != default) editable.PublishedEntityId = publishedId.Value;

        //    if (newId != null)
        //    {
        //        editable.RepositoryId = newId.Value;//note: this was not in before, could cause side-effects
        //    }

        //    return entity;
        //}

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
