using System.Collections.Concurrent;
using ToSic.Eav.Context;
using ToSic.Eav.Data.Attributes.Sys;
using ToSic.Eav.Data.EntityDecorators.Sys;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Serialization;
using ToSic.Lib.Documentation;
using static ToSic.Eav.Serialization.EntitySerializationDecorator;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataFormats.EavLight;

/// <summary>
/// A helper to serialize various combinations of entities, lists of entities etc
/// </summary>
[PrivateApi("Hide Implementation")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class ConvertToEavLight : ServiceBase<ConvertToEavLight.MyServices>, IConvertToEavLight
{
    #region Constructor / DI

    public class MyServices(
        LazySvc<IAppReaderFactory> appReaders,
        IValueConverter valueConverter,
        IZoneCultureResolver zoneCultureResolver)
        : MyServicesBase(connect: [appReaders, valueConverter, zoneCultureResolver])
    {
        public LazySvc<IAppReaderFactory> AppReaders { get; } = appReaders;
        public IValueConverter ValueConverter { get; } = valueConverter;
        public IZoneCultureResolver ZoneCultureResolver { get; } = zoneCultureResolver;
    }

    /// <summary>
    /// Important: this constructor is used both in inherited,
    /// but also in EAV-code which uses only this object (so no inherited)
    /// This is why it must be public, because otherwise it can't be constructed from eav?
    /// </summary>
    /// <param name="services"></param>
    public ConvertToEavLight(MyServices services) : this(services, "Eav.CnvE2D") { }

    private ConvertToEavLight(MyServices services, string logName) : base(services, logName) { }


    #endregion

    #region Configuration

    /// <inheritdoc />
    [WorkInProgressApi("Exact name not final yet")]
    public int MaxItems { get; set;  } = 0;

    /// <inheritdoc/>
    public bool WithGuid { get; set; }
    /// <inheritdoc/>
    public bool WithPublishing { get; private set; }
        
    [PrivateApi] public MetadataForSerialization MetadataFor { get; private set; } = new();
    [PrivateApi] public ISubEntitySerialization Metadata { get; private set; } = new SubEntitySerialization();

    private bool WithEditInfos { get; set; }

    // WIP v17
    public void AddSelectFields(List<string> fields) => _presetFilters = new EntitySerializationDecoratorCreator(fields, WithGuid, Log).Generate();

    protected EntitySerializationDecorator PresetFilters => _presetFilters ??= new EntitySerializationDecoratorCreator(null, WithGuid, Log).Generate();
    private EntitySerializationDecorator _presetFilters;

    /// <inheritdoc/>
    public void ConfigureForAdminUse()
    {
        WithGuid = true;
        WithPublishing = true;
        MetadataFor = new() { Serialize = true };
        Metadata = new SubEntitySerialization { Serialize = true, SerializeId = true, SerializeTitle = true, SerializeGuid = true };
        WithEditInfos = true;
        LinksWithBothValues = true;
    }

    /// <summary>
    /// WIP 17.02+ - should show link as file:42|https://...
    /// </summary>
    private bool LinksWithBothValues { get; set; }

    #endregion

    #region Language

    public string[] Languages
    {
        get => field ??= Services.ZoneCultureResolver.SafeLanguagePriorityCodes();
        set;
    }

    /// <summary>
    /// Separate helper to create lightweight sub entities
    /// </summary>
    internal ConvertToJsonBasicLightSubEntities SubConverter => field ??= new(Languages);


    [PrivateApi("not public ATM")]
    public TypeSerializationOptions Type { get; set; } = new();

    #endregion

    #region Constants

    // TODO: the _Title is probably never used in JS but we must verify
    public const string InternalTitleField = "_Title";
    public const string InternalTypeField = "_Type";
    // TODO: @STV - this looks completely wrong
    // The field names don't always look like this - we must check the field type and possibly a metadata attribute
    //private readonly IEnumerable<string> _notSerializableAttributeNames = new List<string> { "EphemeralString", "Group", "GroupEnd", "Message" };

    #endregion


    /// <summary>
    /// Convert an entity into a lightweight dictionary, ready to serialize
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    [PrivateApi]
    protected virtual EavLightEntity GetDictionaryFromEntity(IEntity entity)
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
            .Where(d => excludeAttributes?.Contains(d.Name) != true)
            .ToList();

        // If we have filter fields from $select, apply that (new ca. v17)
        if (rules.FilterFieldsEnabled == true)
            attributes = attributes
                .Where(a => rules.FilterFields.Any(ff => ff.EqualsInsensitive(a.Name)))
                .ToList();

        var entityValues = attributes
            .ToEavLight(attribute => attribute.Name, attribute =>
            {
                var rawValue = entity.Get(attribute.Name, languages: Languages);

                return attribute.Type switch
                {
                    // Special Case 1: Hyperlink Field which must be resolved
                    ValueTypes.Hyperlink when rawValue is string stringValue
                                              && ValueConverterBase.CouldBeReference(stringValue)
                        => (LinksWithBothValues ? stringValue + "|" : "" ) // Optionally prefix with original value, but only in admin-mode new 17.02+
                           + Services.ValueConverter.ToValue(stringValue, entity.EntityGuid),
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

        if (WithPublishing)
        {
            var appState = Services.AppReaders.Value.Get(entity.AppId);
            AddPublishingInformation(entity, entityValues, appState);
        };

        AddMetadataAndFor(entity, entityValues, rules);

        // Special edit infos - _Title (old, maybe not needed), Stats, EditInfo for read-only etc.
        // 2024-03-05 2dm - basically when the $select is applied, don't add these anymore
        if (rules.FilterFieldsEnabled != true && WithEditInfos)
        {
            // this internal _Title field is probably not used much anymore, so there is no rule for it
            // Probably remove at some time in near future, once verified it's not used in the admin-front-end
            try { entityValues.Add(InternalTitleField, entity.GetBestTitle(Languages)); }
            catch { /* ignore */ }

            AddStatistics(entity, entityValues);
            AddEditInfo(entity, entityValues);
        }

        // Include title field, if there is not already one in the dictionary
        // - If forced, always set/override
        // - if the rules say to include (or no rules), only replace if not already set
        var titleFieldName = rules.CustomTitleName ?? AttributeNames.TitleNiceName;
        if ((rules.SerializeTitleForce ?? DefaultSerializeTitleForce) || ((rules.SerializeTitle ?? DefaultSerializeTitle) && !entityValues.ContainsKey(titleFieldName)))
            entityValues[titleFieldName] = entity.GetBestTitle(Languages);

        AddDateInformation(entity: entity, entityValues: entityValues, rules: rules);

        var tSer = rules.SerializeType;
        if (Type.Serialize)
            entityValues[InternalTypeField] = new JsonType(entity, Type.WithDescription);

        // TODO
        else if (!string.IsNullOrWhiteSpace(tSer?.SerializeAs))
        {
            var propName = tSer.PropertyNames ?? "Type";
            switch (tSer.SerializeAs.ToLower())
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
        

        return entityValues;
    }

    #region Exclude Attributes Cache

    /// <summary>
    /// Get list of attributes to exclude when serializing, especially Empty and Ephemeral attributes
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    private List<string> ExcludeAttributesOfType(IEntity entity)
    {
        // Check if the entity type is in the cache
        if (_excludeAttributesCache.TryGetValue(entity.Type, out var excludeAttributes))
            return excludeAttributes;

        // If it's not in the cache, compute the list of attributes and add it to the cache
        excludeAttributes = entity.Type.Attributes?.ToList()
            .Where(a => a.Type == ValueTypes.Empty
                        || a.Metadata.GetBestValue<bool>(AttributeMetadataConstants.MetadataFieldAllIsEphemeral))
            .Select(a => a.Name)
            .ToList();

        // Cache and return
        _excludeAttributesCache[entity.Type] = excludeAttributes;
        return excludeAttributes;
    }

    // note 2025-01-31: in 19.02 LTS there seems to be an issue on the line
    // _excludeAttributesCache[entity.Type] = excludeAttributes; (ca. 255)
    // ATM we believe it's a concurrency issue, but we're not sure yet.
    // https://stackoverflow.com/questions/1320264/how-did-i-get-this-nullreferenceexception-error-here-right-after-the-constructor
    // So we changed it to a ConcurrentDictionary to see if that helps.
    private readonly ConcurrentDictionary<object, List<string>> _excludeAttributesCache = [];

    #endregion

    private void OptimizeRemoveEmptyValues(EntitySerializationDecorator rules, EavLightEntity entityValues)
    {
        if (rules == null) return;
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

}