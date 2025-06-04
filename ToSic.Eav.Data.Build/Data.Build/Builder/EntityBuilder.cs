using System.Collections.Immutable;
using ToSic.Eav.Data.Entities.Sys;
using ToSic.Eav.Data.Relationships.Sys;
using ToSic.Eav.Metadata;
using ToSic.Lib.Coding;


namespace ToSic.Eav.Data.Build;

/// <summary>
/// This is a helper environment to build entities based on different needs
/// It's basically different kinds of constructors, just to keep the primary 
/// Entity object lean and clean
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class EntityBuilder(AttributeBuilder attributeBuilder)
{
    public Entity Create(
        int appId,
        IContentType contentType,
        NoParamOrder noParamOrder = default,
        IImmutableDictionary<string, IAttribute>? attributes = default,
        int entityId = default,
        int repositoryId = Constants.NullId,
        Guid guid = default,
        string? titleField = default,
        DateTime? created = default,
        DateTime? modified = default,
        string? owner = default,
        int version = default,
        bool isPublished = true,
        ITarget? metadataFor = default,
        EntityPartsLazy? partsBuilder = default,
        int publishedId = default)
    {
        return new()
        {
            AppId = appId,
            EntityId = entityId,
            EntityGuid = guid,
            Type = contentType,
            TitleFieldName = titleField,
            Attributes = attributes ?? AttributeBuilder.EmptyList,

            Created = created ?? default,
            Modified = modified ?? default,
            Owner = owner,
            MetadataFor = metadataFor,
            // Prepare the Parts-builder in case it wasn't provided
            PartsLazy = partsBuilder ?? new(),

            // *** Entity stuff ***
            // RepositoryId should default to EntityId, if not provided
            RepositoryId = repositoryId == Constants.NullId ? entityId : repositoryId,
            // Version should always default to 1, if not provided
            Version = version == default ? 1 : version,
            IsPublished = isPublished,
            PublishedEntityId = publishedId == 0 ? null : (int?)publishedId, // fix: #3070 convert 0 to null 
        };
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
            attributes: attributeBuilder.Create(type, null),
            created: DateTime.MinValue,
            modified: DateTime.Now, 
            owner: "");

    /// <summary>
    /// Create a new Entity based on an Entity and replacing some of its properties
    /// </summary>
    public IEntity CreateFrom(
        IEntity original,
        NoParamOrder noParamOrder = default,
        int? appId = default,
        IImmutableDictionary<string, IAttribute>? attributes = default,
        int? id = default,
        int? repositoryId = default,
        Guid? guid = default,
        IContentType? type = default,
        bool? isPublished = default,
        DateTime? created = default,
        DateTime? modified = default,
        string? owner = default,
        int? version = default,
        ITarget? target = default,

        // publishing Instructions - should go elsewhere
        int? publishedId = default
    )
    {
        // Fresh parts builder for relationships & metadata
        var entityPartsBuilder = EntityPartsBuilder(original, id, guid);

        var asRealEntity = original as Entity;
        var e = Create(
            appId: appId ?? original.AppId,
            attributes: attributes ?? asRealEntity?.Attributes, 
            entityId: id ?? original.EntityId,
            repositoryId: repositoryId ?? original.RepositoryId,
            guid: guid ?? original.EntityGuid,
            contentType: type ?? original.Type,
            titleField: asRealEntity?.TitleFieldName, 
            created: created ?? original.Created,
            modified: modified ?? original.Modified,
            owner: owner ?? original.Owner,
            version: version ?? original.Version,
            metadataFor: target ?? new Target(original.MetadataFor),

            isPublished: isPublished ?? original.IsPublished,
            publishedId: publishedId ?? asRealEntity?.PublishedEntityId ?? default,
                
            partsBuilder: entityPartsBuilder
        );

        return e;
    }

    private static EntityPartsLazy EntityPartsBuilder(IEntity original, int? newId, Guid? newGuid)
    {
        var oldRelManager = (original as Entity)?.Relationships as EntityRelationships;
        var entityPartsBuilder = new EntityPartsLazy(
            ent => EntityRelationships.ForClone(ent, oldRelManager),
            getMetadataOf: newId == default && newGuid == default
                // If identifiers don't change, it will provide the identical metadata
                ? EntityPartsLazy.ReUseMetadataFunc<Guid>(original.Metadata)
                // If they do change, we need to create a derived clone
                : EntityPartsLazy.CloneMetadataFunc<Guid>(original.Metadata)
        );
        return entityPartsBuilder;
    }
}