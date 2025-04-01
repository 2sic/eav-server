using System.Text.Json;
using System.Text.Json.Serialization;
using ToSic.Eav.Data;
using ToSic.Eav.Plumbing;
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
            serializeRelationships: priority.SerializeRelationships,
            serializeType: priority.SerializeType) { }

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
        ISubEntitySerialization serializeRelationships = null,
        TypeSerialization serializeType = default): this()
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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SerializeId { get; init; }

    /// <summary>
    /// Include AppId - not included by default.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SerializeAppId { get; init; }

    /// <summary>
    /// Include ZoneId - not included by default.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SerializeZoneId { get; init; }

    /// <summary>
    /// Include GUID - if not set, is not included.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SerializeGuid { get; init; }

    /// <summary>
    /// Add standard "Title" property - default is ???.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SerializeTitle { get; init; }
    public const bool DefaultSerializeTitle = true;

    /// <summary>
    /// Force add the title - when basically introduced through the $select.
    /// This is important, because of different ways a "Title" field can be "Title" or something else.
    /// So in rare cases, there is a "Title" field but the title itself is a "Name" or "TitleInternal".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SerializeTitleForce { get; init; }
    public const bool DefaultSerializeTitleForce = false;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string CustomTitleName { get; init; }

    /// <summary>
    /// Include Modified date - default is false.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SerializeModified { get; init; }

    /// <summary>
    /// Include Created date - default is false.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? FilterFieldsEnabled { get; init; }

    /// <summary>
    /// WIP v17.04 - Fields to include in the result. Will only take effect if <see cref="FilterFieldsEnabled"/>.
    /// Note: not in the entity config, but ATM added manually from the $select
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> FilterFields { get; init; }

    #region Metadata & Relationships

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MetadataForSerialization SerializeMetadataFor { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ISubEntitySerialization SerializeMetadata { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ISubEntitySerialization SerializeRelationships { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TypeSerialization SerializeType { get; init; }

    #endregion

    #region Generate new Serialize Specs based on URL parameters $select

    /// <summary>
    /// Create a new EntitySerializationDecorator based on the $select parameters in the URL to filter the fields
    /// </summary>
    /// <param name="selectFields">list of fields to select</param>
    /// <param name="withGuid">If it should add the guids by default - usually when the serializer is configured as admin-mode</param>
    /// <param name="log">log to use in this static function</param>
    /// <returns></returns>
    public static EntitySerializationDecorator FromFieldList(List<string> selectFields, bool withGuid, ILog log)
    {
        var l = log.Fn<EntitySerializationDecorator>($"{nameof(selectFields)}: {selectFields}, {nameof(withGuid)}: {withGuid}");
        
        // If nothing, exit. Only specify SerializeGuid if it's true, otherwise null so it can use other defaults
        if (selectFields == null || !selectFields.Any())
            return l.Return(new() { SerializeGuid = withGuid ? true : null }, "no filters");

        // Force adding the title as it's a special case
        // First check for "EntityTitle" - as that would mean we should add it, but rename it
        var dropNames = SystemFields.Keys.ToList();
        bool forceAddTitle;
        string customTitleFieldName = null;
        var fieldsAllLower = selectFields
            .Select(f => f.ToLowerInvariant())
            .ToList();
        if (fieldsAllLower.Contains(EntityFieldTitle.ToLowerInvariant()))
        {
            l.A($"Found long title name {EntityFieldTitle}, will process replacement strategy");
            forceAddTitle = true;
            customTitleFieldName = "EntityTitle"; // can't 
            dropNames.Add(EntityFieldTitle);

            // if we have both EntityTitle and Title, we should NOT drop the Title Nice name
            // Note that this is case-safe, since the original list comes from the system-fields dictionary
            dropNames.Remove(TitleNiceName);
        }
        else
        {
            forceAddTitle = fieldsAllLower.Contains(TitleNiceName.ToLowerInvariant());
        }

        // Simpler fields, these do not have a custom name feature, they are either added or not
        var addId = fieldsAllLower.Contains(IdNiceName.ToLowerInvariant());
        var addGuid = fieldsAllLower.Contains(GuidNiceName.ToLowerInvariant());
        var addModified = fieldsAllLower.Contains(ModifiedNiceName.ToLowerInvariant());
        var addCreated = fieldsAllLower.Contains(CreatedNiceName.ToLowerInvariant());

        // drop all system fields
        var filterFields = selectFields
            .Where(sf => dropNames.Any(key => !key.EqualsInsensitive(sf)))
            .ToList();

        var result = new EntitySerializationDecorator
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
        return l.Return(result, JsonSerializer.Serialize(result));
    }


    #endregion
}