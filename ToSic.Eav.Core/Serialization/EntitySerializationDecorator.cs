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
public class EntitySerializationDecorator(): IDecorator<IEntity>, IEntityIdSerialization
{
    public EntitySerializationDecorator(
        EntitySerializationDecorator original,
        bool? serializeId = null,
        bool? serializeAppId = null,
        bool? serializeZoneId = null,
        bool? serializeGuid = null,
        bool? serializeTitle = null,
        string customTitleName = null,
        bool? serializeModified = null,
        bool? serializeCreated = null,
        bool? removeEmptyStringValues = null,
        bool? removeBoolFalseValues = null,
        bool? removeNullValues = null,
        bool? removeZeroValues = null,
        List<string> filterFields = null,
        MetadataForSerialization serializeMetadataFor = null,
        ISubEntitySerialization serializeMetadata = null,
        ISubEntitySerialization serializeRelationships = null): this()
    {
        SerializeId = serializeId ?? original?.SerializeId;
        SerializeAppId = serializeAppId ?? original?.SerializeAppId;
        SerializeZoneId = serializeZoneId ?? original?.SerializeZoneId;
        SerializeGuid = serializeGuid ?? original?.SerializeGuid;
        SerializeTitle = serializeTitle ?? original?.SerializeTitle;
        CustomTitleName = customTitleName ?? original?.CustomTitleName;
        SerializeModified = serializeModified ?? original?.SerializeModified;
        SerializeCreated = serializeCreated ?? original?.SerializeCreated;
        RemoveEmptyStringValues = removeEmptyStringValues ?? original?.RemoveEmptyStringValues ?? default;
        RemoveBoolFalseValues = removeBoolFalseValues ?? original?.RemoveBoolFalseValues ?? default;
        RemoveNullValues = removeNullValues ?? original?.RemoveNullValues ?? default;
        RemoveZeroValues = removeZeroValues ?? original?.RemoveZeroValues ?? default;
        FilterFields = filterFields ?? original?.FilterFields;
        SerializeMetadataFor = serializeMetadataFor ?? original?.SerializeMetadataFor;
        SerializeMetadata = serializeMetadata ?? original?.SerializeMetadata;
        SerializeRelationships = serializeRelationships ?? original?.SerializeRelationships;
    }

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

    public string CustomTitleName { get; init; } = null;

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