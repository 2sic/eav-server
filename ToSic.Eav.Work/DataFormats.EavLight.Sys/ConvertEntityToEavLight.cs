using System.Collections.Concurrent;
using ToSic.Eav.Apps;
using ToSic.Eav.Context.Sys.ZoneCulture;
using ToSic.Eav.Data.ContentTypes.Fields;
using ToSic.Eav.Data.EntityDecorators.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Data.Sys.ValueConverter;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Serialization.Sys.Options;
using ToSic.Sys.Utils;

namespace ToSic.Eav.DataFormats.EavLight.Sys;

public partial class ConvertEntityToEavLight(
    LazySvc<IAppReaderFactory> appReaders,
    IValueConverter valueConverter,
    IZoneCultureResolver zoneCultureResolver,
    IEnumerable<IConvertEntityToEavLightExtraProcessor> extraProcessors)
    : ServiceBase("Cnv.E2ELight", connect: [appReaders, valueConverter, zoneCultureResolver]), IConvert<IEntity, EavLightEntity>
{
    #region Constants

    // TODO: the _Title is probably never used in JS but we must verify
    public const string InternalTitleField = "_Title";
    public const string InternalTypeField = "_Type";
    // TODO: @STV - this looks completely wrong
    // The field names don't always look like this - we must check the field type and possibly a metadata attribute
    //private readonly IEnumerable<string> _notSerializableAttributeNames = new List<string> { "EphemeralString", "Group", "GroupEnd", "Message" };

    #endregion


    /// <inheritdoc/>
    public IEnumerable<EavLightEntity> Convert(IEnumerable<IEntity> entities)
    {
        var l = Log.Fn<IEnumerable<EavLightEntity>>(timer: true);
        var topEntities = MaxItems == 0
            ? entities
            : entities.Take(MaxItems);
        var result = topEntities
            .Select(GetDictionaryFromEntity)
            .ToListOpt();
        return l.ReturnAsOk(result);
    }

    /// <inheritdoc/>
    public EavLightEntity Convert(IEntity entity)
    {
        var l = Log.Fn<EavLightEntity>(timer: true);
        var result = entity == null!
            ? null!
            : GetDictionaryFromEntity(entity);
        return l.ReturnAsOk(result);
    }

    public IEnumerable<EavLightEntity> Convert(IEnumerable<IEntityWrapper> list)
        => Convert(list.Select(item => item.Entity));

    /// <inheritdoc />
    public EavLightEntity Convert(IEntityWrapper item)
        => Convert(item.Entity);



    /// <inheritdoc />
    [WorkInProgressApi("Exact name not final yet")]
    public int MaxItems { get; set; } = 0;

    /// <inheritdoc/>
    public bool WithGuid { get; set; }

    /// <inheritdoc/>
    public bool WithPublishing { get; set; }


    [field: AllowNull, MaybeNull]
    public string?[] Languages
    {
        get => field ??= zoneCultureResolver.SafeLanguagePriorityCodes();
        set;
    }

    public TypeSerializationOptions Type { get; set; } = new();

    /// <summary>
    /// Separate helper to create lightweight sub entities
    /// </summary>
    [PrivateApi("Actually Internal!")]
    [field: AllowNull, MaybeNull]
    ConvertToJsonBasicLightSubEntities SubConverter => field ??= new(Languages);

    /// <summary>
    /// Convert an entity into a lightweight dictionary, ready to serialize
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    [PrivateApi]
    public EavLightEntity GetDictionaryFromEntity(IEntity entity)
    {
        // Get serialization rules if some exist - new in 11.13
        // They can be different for each entity, so we must get them from the entity
        var attachedRules = entity.GetDecorator<EntitySerializationDecorator>();
        var rules = new EntitySerializationDecorator(PresetFilters, attachedRules);

        // Figure out how to serialize relationships
        var serRels = SubEntitySerialization.Stabilize(
            original: rules.SerializeRelationships,
            serialize: true,
            format: "object",
            id: true,
            guid: false,
            title: true
        );

        var excludeAttributes = ExcludeAttributesOfType(entity);

        // Convert Entity to dictionary
        // If the value is a relationship, then give those too, but only Title and Id
        var attributes = entity.Attributes
            .Select(d => d.Value)
            .Where(d => excludeAttributes.Contains(d.Name) != true)
            .ToListOpt();

        // If we have filter fields from $select, apply that (new ca. v17)
        if (rules.FilterFieldsEnabled == true)
            attributes = attributes
                .Where(a => rules.FilterFields!.Any(ff => ff.EqualsInsensitive(a.Name)))
                .ToListOpt();

        var useLinkWithBothValues = LinksWithBothValues || rules.LinksWithBothValues == true;
        var entityValues = attributes
            .ToEavLight(
                attribute => attribute.Name,
                attribute =>
                {
                    var rawValue = entity.Get(attribute.Name, languages: Languages);

                    return attribute.Type switch
                    {
                        // Special Case 1: Hyperlink Field which must be resolved
                        ValueTypes.Hyperlink when rawValue is string stringValue
                                                  && ValueConverterBase.CouldBeReference(stringValue)
                            => (useLinkWithBothValues
                                   ? stringValue + "|"
                                   : "") // Optionally prefix with original value, but only in admin-mode new 17.02+
                               + valueConverter.ToValue(stringValue, entity.EntityGuid),
                        // Special Case 2: Entity-List
                        ValueTypes.Entity when rawValue is IEnumerable<IEntity> entities
                            => serRels.Serialize == true
                                ? SubConverter.CreateListOrCsvOfSubEntities(entities, serRels)
                                : null,
                        _ => rawValue
                    };

                    // Default: Normal Value
                });

        // todo: verify what happens with null-values on the relationships, maybe we should filter them out again?

        // New 12.05 - drop null values as specified in the configuration - use attached rules if they exist
        // don't use the final rules, as they don't affect these settings as of now
        if (attachedRules != null)
            OptimizeRemoveEmptyValues(attachedRules, entityValues);

        // Add Id, Guid, AppId - according to rules
        AddAllIds(entity, entityValues, rules, withGuidFallback: WithGuid);

        if (WithPublishing || rules.WithPublishing == true)
        {
            var appState = appReaders.Value.Get(entity.AppId);
            AddPublishingInformation(entity, entityValues, appState);
        }

        AddMetadataAndFor(entity, entityValues, rules);

        // #SysData WIP
        var useEditInfos = rules.WithEditInfos == true;

        // Special edit infos - _Title (old, maybe not needed), Stats, EditInfo for read-only etc.
        // 2024-03-05 2dm - basically when the $select is applied, don't add these anymore
        if (rules.FilterFieldsEnabled != true && useEditInfos)
        {
            // this internal _Title field is probably not used much anymore, so there is no rule for it
            // Probably remove at some time in near future, once verified it's not used in the admin-front-end ⚠️
            try { entityValues.Add(InternalTitleField, entity.GetBestTitle(Languages)); }
            catch { /* ignore */ }

            AddStatistics(entity, entityValues);
            AddEditInfo(entity, entityValues);
        }

        // Include title field, if there is not already one in the dictionary
        // - If forced, always set/override
        // - if the rules say to include (or no rules), only replace if not already set
        var titleFieldName = rules.CustomTitleName ?? AttributeNames.TitleNiceName;
        if ((rules.SerializeTitleForce ?? EntitySerializationDecorator.DefaultSerializeTitleForce) || ((rules.SerializeTitle ?? EntitySerializationDecorator.DefaultSerializeTitle) && !entityValues.ContainsKey(titleFieldName)))
            entityValues[titleFieldName] = entity.GetBestTitle(Languages);

        AddDateInformation(entity: entity, entityValues: entityValues, rules: rules);

        var tSer = rules.SerializeType;
        if (Type.Serialize)
            entityValues[InternalTypeField] = new JsonType(entity, Type.WithDescription);

        // TODO
        else if (!string.IsNullOrWhiteSpace(tSer?.SerializeAs))
        {
            var propName = tSer!.PropertyNames ?? "Type";
            switch (tSer.SerializeAs!.ToLower())
            {
                case "object":
                    entityValues[propName] = new JsonType(entity, tSer.SerializeDescription == true);
                    break;
                case "flat":
                    if (tSer.SerializeId == true)
                        entityValues[$"{propName}Id"] = entity.Type.NameId;
                    if (tSer.SerializeName == true)
                        entityValues[$"{propName}Name"] = entity.Type.Name;
                    break;
                default:
                    Log.W($"Unknown serialization type: {Type.SerializeAs}");
                    break;
            }

        }

        foreach (var convertEntityToEavLightExtraProcessor in extraProcessors)
        {
            entityValues = convertEntityToEavLightExtraProcessor.Process(this, entity, entityValues);
        }

        return entityValues;
    }

    [PrivateApi("Actually Internal!")]
    public void AddSelectFields(ICollection<string> fields)
        => PresetFilters = new EntitySerializationDecoratorCreator(fields, WithGuid, Log).Generate();

    internal EntitySerializationDecorator PresetFilters
    {
        get => field ??= new EntitySerializationDecoratorCreator(null, WithGuid, Log).Generate();
        private set;
    }

    /// <summary>
    /// WIP 17.02+ - should show link as file:42|https://...
    /// </summary>
    private bool LinksWithBothValues { get; set; }


    private void OptimizeRemoveEmptyValues(EntitySerializationDecorator rules, EavLightEntity entityValues)
    {
        var dropNulls = rules.RemoveNullValues;
        var dropZeros = rules.RemoveZeroValues;
        var dropEmptyStrings = rules.RemoveEmptyStringValues;
        var dropBoolFalse = rules.RemoveBoolFalseValues;

        try
        {
            if (dropNulls)
                entityValues
                    .Where(vp => vp.Value == null)
                    .ToList()
                    .ForEach(vp => entityValues.Remove(vp.Key));
        }
        catch (Exception e)
        {
            Log.A("Couldn't drop NULL values, will ignore and continue");
            Log.Ex(e);
        }

        try
        {
            if (dropZeros)
                entityValues
                    .Where(vp =>
                        vp.Value is IConvertible convertible && convertible.IsNumeric() &&
                        System.Convert.ToDouble(convertible) == 0)
                    .ToList()
                    .ForEach(vp => entityValues.Remove(vp.Key));
        }
        catch (Exception e)
        {
            Log.A("Couldn't drop ZERO values, will ignore and continue");
            Log.Ex(e);
        }

        try
        {
            if (dropEmptyStrings)
                entityValues
                    .Where(vp => vp.Value as string == string.Empty)
                    .ToList()
                    .ForEach(vp => entityValues.Remove(vp.Key));
        }
        catch (Exception e)
        {
            Log.A("Couldn't drop EMPTY string values, will ignore and continue");
            Log.Ex(e);
        }

        try
        {
            if (dropBoolFalse)
                entityValues
                    .Where(vp => vp.Value is bool boolVal && boolVal == false)
                    .ToList()
                    .ForEach(vp => entityValues.Remove(vp.Key));
        }
        catch (Exception e)
        {
            Log.A("Couldn't drop FALSE boolean values, will ignore and continue");
            Log.Ex(e);
        }
    }

    #region Exclude Attributes Cache

    /// <summary>
    /// Get list of attributes to exclude when serializing, especially Empty and Ephemeral attributes
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    private ICollection<string> ExcludeAttributesOfType(IEntity entity)
    {
        // Check if the entity type is in the cache
        if (_excludeAttributesCache.TryGetValue(entity.Type, out var excludeAttributes))
            return excludeAttributes;

        // If it's not in the cache, compute the list of attributes and add it to the cache
        excludeAttributes = entity.Type.Attributes
                                ?.ToListOpt()
                                .Where(a => a.Type == ValueTypes.Empty
                                            || a.Metadata.Get<bool>(nameof(IFieldSettingsGeneral.IsEphemeral)))
                                .Select(a => a.Name)
                                .ToListOpt()
                            ?? [];

        // Cache and return
        _excludeAttributesCache[entity.Type] = excludeAttributes;
        return excludeAttributes;
    }

    // note 2025-01-31: in 19.02 LTS there seems to be an issue on the line
    // _excludeAttributesCache[entity.Type] = excludeAttributes; (ca. 255)
    // ATM we believe it's a concurrency issue, but we're not sure yet.
    // https://stackoverflow.com/questions/1320264/how-did-i-get-this-nullreferenceexception-error-here-right-after-the-constructor
    // So we changed it to a ConcurrentDictionary to see if that helps.
    private readonly ConcurrentDictionary<object, ICollection<string>> _excludeAttributesCache = [];

    #endregion



}