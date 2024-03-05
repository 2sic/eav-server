﻿using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;
using static ToSic.Eav.Data.Attributes;

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
    public EntitySerializationDecorator(EntitySerializationDecorator priority, EntitySerializationDecorator mixin)
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
            serializeRelationships: priority.SerializeRelationships) { }

    public EntitySerializationDecorator(
        EntitySerializationDecorator original,
        bool? serializeId = null,
        bool? serializeAppId = null,
        bool? serializeZoneId = null,
        bool? serializeGuid = null,
        bool? serializeTitle = null,
        bool? serializeTitleForce = null,
        string customTitleName = null,
        bool? serializeModified = null,
        bool? serializeCreated = null,
        bool? removeEmptyStringValues = null,
        bool? removeBoolFalseValues = null,
        bool? removeNullValues = null,
        bool? removeZeroValues = null,
        List<string> filterFields = null,
        bool? filterFieldsEnabled = null,
        MetadataForSerialization serializeMetadataFor = null,
        ISubEntitySerialization serializeMetadata = null,
        ISubEntitySerialization serializeRelationships = null): this()
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
    }

    /// <summary>
    /// Include ID - if not set, is included.
    /// </summary>
    public bool? SerializeId { get; init; }

    /// <summary>
    /// Include AppId - not included by default.
    /// </summary>
    public bool? SerializeAppId { get; init; }

    /// <summary>
    /// Include ZoneId - not included by default.
    /// </summary>
    public bool? SerializeZoneId { get; init; }

    /// <summary>
    /// Include GUID - if not set, is not included.
    /// </summary>
    public bool? SerializeGuid { get; init; }

    /// <summary>
    /// Add standard "Title" property - default is ???.
    /// </summary>
    public bool? SerializeTitle { get; init; }
    public const bool DefaultSerializeTitle = true;

    /// <summary>
    /// Force add the title - when basically introduced through the $select.
    /// This is important, because of different ways a "Title" field can be "Title" or something else.
    /// So in rare cases, there is a "Title" field but the title itself is a "Name" or "TitleInternal".
    /// </summary>
    public bool? SerializeTitleForce { get; init; }
    public const bool DefaultSerializeTitleForce = false;

    public string CustomTitleName { get; init; }

    /// <summary>
    /// Include Modified date - default is false.
    /// </summary>
    public bool? SerializeModified { get; init; }

    /// <summary>
    /// Include Created date - default is false.
    /// </summary>
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
    public bool? FilterFieldsEnabled { get; init; }

    /// <summary>
    /// WIP v17.04 - Fields to include in the result. Will only take effect if <see cref="FilterFieldsEnabled"/>.
    /// Note: not in the entity config, but ATM added manually from the $select
    /// </summary>
    public List<string> FilterFields { get; init; }

    #region Metadata & Relationships

    public MetadataForSerialization SerializeMetadataFor { get; init; }

    public ISubEntitySerialization SerializeMetadata { get; init; }

    public ISubEntitySerialization SerializeRelationships { get; init; }

    #endregion

    #region Generate new Serialize Specs based on URL parameters $select

    /// <summary>
    /// Create a new EntitySerializationDecorator based on the $select parameters in the URL to filter the fields
    /// </summary>
    /// <param name="selectFields">list of fields to select</param>
    /// <param name="withGuid">If it should add the guids by default - usually when the serializer is configured as admin-mode</param>
    /// <returns></returns>
    public static EntitySerializationDecorator FromFieldList(List<string> selectFields, bool withGuid)
    {
        // If nothing, exit.
        if (selectFields == null || !selectFields.Any())
            return new() { SerializeGuid = withGuid };

        // Force adding the title as it's a special case
        // First check for "EntityTitle" - as that would mean we should add it, but rename it
        var dropNames = SystemFields.Keys.ToList();
        bool forceAddTitle;
        string customTitleFieldName = null;
        if (selectFields.Any(sf => sf.EqualsInsensitive(EntityFieldTitle)))
        {
            forceAddTitle = true;
            customTitleFieldName = "EntityTitle"; // can't 
            dropNames.Add(EntityFieldTitle);

            // if we have both EntityTitle and Title, we should NOT drop the Title Nice name
            // Note that this is case-safe, since the original list comes from the system-fields dictionary
            dropNames.Remove(TitleNiceName);
        }
        else
        {
            forceAddTitle = selectFields.Any(sf => sf.EqualsInsensitive(TitleNiceName));
        }

        // Simpler fields, these do not have a custom name feature, they are either added or not
        var addId = selectFields.Any(sf => sf.EqualsInsensitive(IdNiceName));
        var addGuid = selectFields.Any(sf => sf.EqualsInsensitive(GuidNiceName));
        var addModified = selectFields.Any(sf => sf.EqualsInsensitive(ModifiedNiceName));
        var addCreated = selectFields.Any(sf => sf.EqualsInsensitive(CreatedNiceName));

        // drop all system fields
        var filterFields = selectFields.Where(sf => dropNames.Any(key => !key.EqualsInsensitive(sf))).ToList();

        return new()
        {
            SerializeId = addId,
            SerializeGuid = addGuid,
            SerializeModified = addModified,
            SerializeCreated = addCreated,
            SerializeTitleForce = forceAddTitle,
            CustomTitleName = customTitleFieldName,
            FilterFieldsEnabled = true,
            FilterFields = filterFields
        };
    }


    #endregion
}