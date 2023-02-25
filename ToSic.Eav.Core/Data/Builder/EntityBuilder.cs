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
                guid: guid, titleAttribute: titleField,
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
            //var e = EntityWithAllIdsAndType(appId, entityGuid, entityId, repositoryId,
            //    type, isPublished, created, modified, owner, version, values: values, titleField: titleField);

            //e.MetadataFor = metadataFor;

            //e.Relationships = new RelationshipManager(e, source, null);

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
            //var ent = EntityWithAllIdsAndType(appId, entityGuid, entityId, entityId, 
            //    type, true, DateTime.MinValue, DateTime.Now, "", 1,
            //    values: specs.All, titleField: specs.Title);

            //ent.MetadataFor = new Target();

            //return ent;
        }

        //private Entity EntityWithAllIdsAndType(int appId, Guid entityGuid, int entityId,
        //    int repositoryId, IContentType type, bool isPublished,
        //    DateTime created,
        //    DateTime modified, string owner, int version, 
        //    Dictionary<string, IAttribute> values = default,
        //    string titleField = default) =>
        //    Create(appId: appId,
        //        values: null,
        //        typedValues: values, 
        //        entityId: entityId,
        //        version: version,
        //        guid: entityGuid,
        //        contentType: type,
        //        isPublished: isPublished,
        //        repositoryId: repositoryId,
        //        created: created,
        //        modified: modified,
        //        owner: owner,
        //        titleField: titleField
        //    );

        /// <summary>
        /// Create a new Entity based on an Entity and Attributes
        /// Used in the Attribute-Filter, which generates a new entity with less properties
        /// </summary>
        public Entity Clone(IEntity entity, 
            Dictionary<string, IAttribute> newValues, 
            IEnumerable<EntityRelationship> entityRelationshipsIfNoApp = default,
            int? newId = default,
            int? newRepoId = default,
            Guid? newGuid = default,
            IContentType newType = default)
        {
            var entityPartsBuilder = new EntityPartsBuilder(
                ent =>
                {
                    var lookupApp2 = (entity as Entity)?.DeferredLookupData as AppState;
                    return new RelationshipManager(ent, lookupApp2, entityRelationshipsIfNoApp);
                }
            );

            var targetType = newType ?? entity.Type;

            var e = Create(appId: entity.AppId,
                values: null,
                typedValues: newValues,
                entityId: newId ?? entity.EntityId,
                repositoryId: newRepoId ?? entity.RepositoryId,
                guid: newGuid ?? entity.EntityGuid,
                contentType: targetType, titleField: entity.Title?.Name,
                isPublished: entity.IsPublished,
                created: entity.Created, modified: entity.Modified,
                owner: entity.Owner, version: entity.Version,
                metadataFor: new Target(entity.MetadataFor),
                partsBuilder: entityPartsBuilder
            );
            //var e = EntityWithAllIdsAndType(entity.AppId, entity.EntityGuid, entity.EntityId, entity.RepositoryId, targetType, 
            //    entity.IsPublished, entity.Created, entity.Modified, entity.Owner, entity.Version, attributes);
            //e.TitleFieldName = entity.Title?.Name;
            //e.MetadataFor = new Metadata.Target(entity.MetadataFor);

            var lookupApp = (entity as Entity)?.DeferredLookupData as AppState;
            //e.Relationships = new RelationshipManager(e, lookupApp, entityRelationshipsIfNoApp);


            e.DeferredLookupData = lookupApp;
            return e;
        }

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
