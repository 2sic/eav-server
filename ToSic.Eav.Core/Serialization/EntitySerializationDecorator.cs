using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Serialization;

/// <summary>
/// An entity should be able to specify if some properties should not be included
/// </summary>
/// <remarks>
/// * Introduced v11.13 in a slightly different implementation
/// * Enhanced as a standalone decorator in 12.05
/// </remarks>
[PrivateApi("Just fyi, was previously published as internal till v14")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class EntitySerializationDecorator: IDecorator<IEntity>, IEntityIdSerialization
{
    /// <summary>
    /// Include ID - if not set, is included.
    /// </summary>
    public bool? SerializeId { get; init; } = null;

    /// <summary>
    /// Include AppId - not included by default.
    /// </summary>
    public bool? SerializeAppId { get; init; } = null;

    /// <summary>
    /// Include ZoneId - not included by default.
    /// </summary>
    public bool? SerializeZoneId { get; init; } = null;

    /// <summary>
    /// Include GUID - if not set, is not included.
    /// </summary>
    public bool? SerializeGuid { get; init; } = null;

    /// <summary>
    /// Add standard "Title" property - default is ???.
    /// </summary>
    public bool? SerializeTitle { get; init; } = null;

    /// <summary>
    /// Include Modified date - default is false.
    /// </summary>
    public bool? SerializeModified { get; init; } = null;

    /// <summary>
    /// Include Created date - default is false.
    /// </summary>
    public bool? SerializeCreated { get; init; } = null;

    /// <summary>
    /// Lighten the result by dropping empty values; default is false.
    /// </summary>
    public bool RemoveEmptyStringValues { get; init; } = false;

    /// <summary>
    /// Lighten the result by dropping false values; default is false.
    /// </summary>
    public bool RemoveBoolFalseValues { get; init; } = false;

    /// <summary>
    /// Lighten the result by dropping null values; default is false.
    /// </summary>
    public bool RemoveNullValues { get; init; } = false;

    /// <summary>
    /// Lighten the result by dropping 0 values; default is false.
    /// </summary>
    public bool RemoveZeroValues { get; init; } = false;

    /// <summary>
    /// WIP v17.00 - not implemented yet
    /// </summary>
    public List<string> FilterFields { get; init; } = null;

    #region Metadata & Relationships

    public MetadataForSerialization SerializeMetadataFor { get; init; } = null;

    public ISubEntitySerialization SerializeMetadata { get; init; } = null;

    public ISubEntitySerialization SerializeRelationships { get; init; } = null;

    #endregion
}