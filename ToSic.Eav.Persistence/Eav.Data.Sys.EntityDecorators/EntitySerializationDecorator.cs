﻿using ToSic.Eav.Serialization.Sys.Options;

namespace ToSic.Eav.Data.EntityDecorators.Sys;

/// <summary>
/// An entity should be able to specify if some properties should not be included
/// </summary>
/// <remarks>
/// * Introduced v11.13 in a slightly different implementation
/// * Enhanced as a standalone decorator in 12.05
/// </remarks>
[PrivateApi("Just fyi, was previously published as internal till v14")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class EntitySerializationDecorator(): IDecorator<IEntity>, IEntityIdSerialization
{
    public EntitySerializationDecorator(EntitySerializationDecorator priority, EntitySerializationDecorator? mixin)
        : this(original: mixin,
            serializeId: priority.SerializeId,
            serializeAppId: priority.SerializeAppId,
            serializeZoneId: priority.SerializeZoneId,
            serializeGuid: priority.SerializeGuid,
            serializeTitle: priority.SerializeTitle,
            serializeTitleForce: priority.SerializeTitleForce,
            customTitleName: priority.CustomTitleName,
            serializeModified: priority.SerializeModified,
            serializeCreated: priority.SerializeCreated,
            removeEmptyStringValues: priority.RemoveEmptyStringValues,
            removeBoolFalseValues: priority.RemoveBoolFalseValues,
            removeNullValues: priority.RemoveNullValues,
            removeZeroValues: priority.RemoveZeroValues,
            filterFields: priority.FilterFields,
            filterFieldsEnabled: priority.FilterFieldsEnabled,
            serializeMetadataFor: priority.SerializeMetadataFor,
            serializeMetadata: priority.SerializeMetadata,
            serializeRelationships: priority.SerializeRelationships,
            serializeType: priority.SerializeType) { }

    public EntitySerializationDecorator(
        EntitySerializationDecorator? original,
        bool? serializeId = null,
        bool? serializeAppId = null,
        bool? serializeZoneId = null,
        bool? serializeGuid = null,
        bool? serializeTitle = null,
        bool? serializeTitleForce = null,
        string? customTitleName = null,
        bool? serializeModified = null,
        bool? serializeCreated = null,
        bool? removeEmptyStringValues = null,
        bool? removeBoolFalseValues = null,
        bool? removeNullValues = null,
        bool? removeZeroValues = null,
        IList<string>? filterFields = null,
        bool? filterFieldsEnabled = null,
        MetadataForSerialization? serializeMetadataFor = null,
        ISubEntitySerialization? serializeMetadata = null,
        ISubEntitySerialization? serializeRelationships = null,
        TypeSerializationOptions? serializeType = default): this()
    {
        SerializeId = serializeId ?? original?.SerializeId;
        SerializeAppId = serializeAppId ?? original?.SerializeAppId;
        SerializeZoneId = serializeZoneId ?? original?.SerializeZoneId;
        SerializeGuid = serializeGuid ?? original?.SerializeGuid;
        SerializeTitle = serializeTitle ?? original?.SerializeTitle;
        SerializeTitleForce = serializeTitleForce ?? original?.SerializeTitleForce;
        CustomTitleName = customTitleName ?? original?.CustomTitleName;
        SerializeModified = serializeModified ?? original?.SerializeModified;
        SerializeCreated = serializeCreated ?? original?.SerializeCreated;
        RemoveEmptyStringValues = removeEmptyStringValues ?? original?.RemoveEmptyStringValues ?? default;
        RemoveBoolFalseValues = removeBoolFalseValues ?? original?.RemoveBoolFalseValues ?? default;
        RemoveNullValues = removeNullValues ?? original?.RemoveNullValues ?? default;
        RemoveZeroValues = removeZeroValues ?? original?.RemoveZeroValues ?? default;
        FilterFields = filterFields ?? original?.FilterFields;
        FilterFieldsEnabled = filterFieldsEnabled ?? original?.FilterFieldsEnabled;
        SerializeMetadataFor = serializeMetadataFor ?? original?.SerializeMetadataFor;
        SerializeMetadata = serializeMetadata ?? original?.SerializeMetadata;
        SerializeRelationships = serializeRelationships ?? original?.SerializeRelationships;
        SerializeType = serializeType ?? original?.SerializeType;
    }

    /// <summary>
    /// Include ID - if not set, is included.
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    public bool? SerializeId { get; init; }

    /// <summary>
    /// Include AppId - not included by default.
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    public bool? SerializeAppId { get; init; }

    /// <summary>
    /// Include ZoneId - not included by default.
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    public bool? SerializeZoneId { get; init; }

    /// <summary>
    /// Include GUID - if not set, is not included.
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    public bool? SerializeGuid { get; init; }

    /// <summary>
    /// Add standard "Title" property - default is ???.
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    public bool? SerializeTitle { get; init; }
    public const bool DefaultSerializeTitle = true;

    /// <summary>
    /// Force add the title - when basically introduced through the $select.
    /// This is important, because of different ways a "Title" field can be "Title" or something else.
    /// So in rare cases, there is a "Title" field but the title itself is a "Name" or "TitleInternal".
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    public bool? SerializeTitleForce { get; init; }
    public const bool DefaultSerializeTitleForce = false;

    [JsonIgnore(Condition = WhenWritingNull)]
    public string? CustomTitleName { get; init; }

    /// <summary>
    /// Include Modified date - default is false.
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    public bool? SerializeModified { get; init; }

    /// <summary>
    /// Include Created date - default is false.
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    public bool? SerializeCreated { get; init; }

    /// <summary>
    /// Lighten the result by dropping empty values; default is false.
    /// </summary>
    public bool RemoveEmptyStringValues { get; init; }

    /// <summary>
    /// Lighten the result by dropping false values; default is false.
    /// </summary>
    public bool RemoveBoolFalseValues { get; init; }

    /// <summary>
    /// Lighten the result by dropping null values; default is false.
    /// </summary>
    public bool RemoveNullValues { get; init; }

    /// <summary>
    /// Lighten the result by dropping 0 values; default is false.
    /// </summary>
    public bool RemoveZeroValues { get; init; }

    /// <summary>
    /// WIP v17.04 - not in the entity config, but ATM added manually from the $select
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    public bool? FilterFieldsEnabled { get; init; }

    /// <summary>
    /// WIP v17.04 - Fields to include in the result. Will only take effect if <see cref="FilterFieldsEnabled"/>.
    /// Note: not in the entity config, but ATM added manually from the $select
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]
    public IList<string>? FilterFields { get; init; }

    #region Metadata & Relationships

    [JsonIgnore(Condition = WhenWritingNull)]
    public MetadataForSerialization? SerializeMetadataFor { get; init; }

    [JsonIgnore(Condition = WhenWritingNull)]
    public ISubEntitySerialization? SerializeMetadata { get; init; }

    [JsonIgnore(Condition = WhenWritingNull)]
    public ISubEntitySerialization? SerializeRelationships { get; init; }

    [JsonIgnore(Condition = WhenWritingNull)]
    public TypeSerializationOptions? SerializeType { get; init; }

    #endregion

}