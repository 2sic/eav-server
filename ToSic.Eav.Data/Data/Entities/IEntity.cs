using ToSic.Eav.Apps;
using ToSic.Eav.Data.Sys.Attributes;
using ToSic.Eav.Data.Sys.PropertyLookup;
using ToSic.Eav.Metadata;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Data;

/// <summary>
/// The primary data-item in the system, IEntity is a generic data-item for any kind of information.
/// > We recommend you read about the [](xref:Basics.Data.Index)
/// </summary>
[PublicApi]
public interface IEntity: IAppIdentityLight, IPublish, IHasPermissions, IPropertyLookup, IHasMetadata, ICanBeEntity
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

    /// <summary>
    /// Best way to get the current entities title.
    /// The field used is determined in the <see cref="IContentType"/>.
    /// If you need the attribute-object, use the <see cref="Title"/> instead.
    /// </summary>
    /// <returns>
    /// The entity title as a string.
    /// </returns>
    string? GetBestTitle();


    /// <summary>
    /// Best way to get the current entities title
    /// </summary>
    /// <param name="dimensions">Array of dimensions/languages to use in the lookup</param>
    /// <returns>The entity title as a string</returns>
    string? GetBestTitle(string?[] dimensions);

    /// <summary>
    /// All the attributes of the current Entity.
    /// </summary>
    IReadOnlyDictionary<string, IAttribute> Attributes { get; }

    /// <summary>
    /// Gets the "official" Title-Attribute <see cref="IAttribute{T}"/>
    /// </summary>
    /// <returns>
    /// The title of this Entity.
    /// The field used is determined in the <see cref="IContentType"/>.
    /// If you need a string, use GetBestTitle() instead.
    /// </returns>
    IAttribute? Title { get; }

    /// <summary>
    /// Gets an Attribute using its StaticName
    /// </summary>
    /// <param name="attributeName">StaticName of the Attribute</param>
    /// <returns>A typed Attribute Object</returns>
    IAttribute? this[string attributeName] { get; }

    /// <summary>
    /// version of this entity in the repository
    /// </summary>
    /// <returns>The version number.</returns>
    int Version { get; }


    /// <summary>
    /// Get the metadata for this item
    /// </summary>
    /// <remarks>
    /// The metadata is either already prepared, from the same app, or from a remote app
    /// </remarks>
    /// <returns>A typed Metadata provider for this Entity</returns>
#pragma warning disable CS0108, CS0114
    IMetadata Metadata { get; }
#pragma warning restore CS0108, CS0114

    #region Children & Parents

    /// <summary>
    /// Get all the children <see cref="IEntity"/> items - optionally only of a specific field and/or type
    /// </summary>
    /// <param name="field">Optional field name to access</param>
    /// <param name="type">Optional type to filter for</param>
    /// <returns>List of children, or empty list if not found. List can contain nulls, as there may be "empty" slots.</returns>
    IEnumerable<IEntity?> Children(string? field = null, string? type = null);

    /// <summary>
    /// Get all the parent <see cref="IEntity"/> items - optionally only of a specific type and/or referenced in a specific field
    /// </summary>
    /// <param name="type">The type name to filter for</param>
    /// <param name="field">The field name where a parent references this item</param>
    /// <returns>List of children, or empty list if not found</returns>
    IEnumerable<IEntity> Parents(string? type = null, string? field = null);

    #endregion

    #region Get (new replacement for Value - more consistent API)

    /// <summary>
    /// Get a value typed as object from this entity.
    /// </summary>
    /// <param name="name">the field/attribute name</param>
    /// <remarks>
    /// * Introduced as beta in 15.06, published in v17
    /// * If you want to supply a `fallback` it will automatically use the generic version of this method
    /// </remarks>
    [PublicApi]
    object? Get(string name);

    /// <summary>
    /// Get a value typed as object from this entity.
    /// </summary>
    /// <param name="name">the field/attribute name</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="language">optional language like `en-us`</param>
    /// <param name="languages">optional list of language IDs which can be a list which is checked in the order provided</param>
    /// <returns></returns>
    /// <remarks>
    /// * Introduced as beta in 15.06, published in v17
    /// * If you want to supply a `fallback` it will automatically use the generic version of this method
    /// </remarks>
    [PublicApi]
    // ReSharper disable once MethodOverloadWithOptionalParameter
    object? Get(string name, NoParamOrder npo = default, string? language = default, string?[]? languages = default);

    #endregion

}