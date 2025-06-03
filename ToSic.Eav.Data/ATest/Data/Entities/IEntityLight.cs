using ToSic.Eav.Apps;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data;

/// <summary>
/// Represents a light Entity, which is a very basic entity
/// without multi-language capabilities, versions or publishing.
/// For the more powerful Entity, use <see cref="IEntity"/>.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
public interface IEntityLight: IAppIdentityLight
{
    #region Identifiers and simple Properties: EntityId, EntityGuid, EntityType, Modified, Created

    /// <summary>
    /// Gets the EntityId
    /// </summary>
    /// <returns>The internal EntityId - usually for reference in the DB, but not quite always (like when this is a draft entity).</returns>
    int EntityId { get; }

    /// <summary>
    /// Gets the EntityGuid
    /// </summary>
    /// <returns>The GUID of the Entity</returns>
    Guid EntityGuid { get; }

    /// <summary>
    /// Gets the ContentType of this Entity
    /// </summary>
    /// <returns>The content-type object.</returns>
    IContentType Type { get; }

    /// <summary>
    /// Gets the Last Modified DateTime
    /// </summary>
    /// <returns>A date-time object.</returns>
    DateTime Modified { get; }
        
    /// <summary>
    /// Gets the Created DateTime
    /// </summary>
    /// <returns>A date-time object.</returns>
    DateTime Created { get; }

    #endregion

    #region Relationships: MetadataFor and Relationships

    /// <summary>
    /// Information which is relevant if this current entity is actually mapped to something else.
    /// If it is mapped, then it's describing another thing, which is identified in this MetadataFor.
    /// </summary>
    /// <returns>A <see cref="ITarget"/> object describing the target.</returns>
    ITarget MetadataFor { get; }

    /// <summary>
    /// Relationship-helper object, important to navigate to children and parents
    /// </summary>
    /// <returns>The <see cref="IEntityRelationships"/> in charge of relationships for this Entity.</returns>
    IEntityRelationships Relationships { get; }

    #endregion


    #region Owner / OwnerId

    /// <summary>
    /// Owner of this entity
    /// </summary>
    /// <returns>A string identifying the owner. Uses special encoding to work with various user-ID providers.</returns>
    string Owner { get; }

    /// <summary>
    /// Owner of this entity - as an int-ID
    /// </summary>
    /// <returns>
    /// This is based on the <see cref="Owner"/> but will only return the ID
    /// </returns>
    /// <remarks>
    /// Added in v15.03
    /// </remarks>
    int OwnerId { get; }

    #endregion

}