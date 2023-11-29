using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Serialization;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataFormats.EavLight;

/// <summary>
/// A helper to serialize various combinations of entities, lists of entities etc
/// </summary>
[PrivateApi("Hide Implementation")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ConvertToEavLight : ServiceBase<ConvertToEavLight.MyServices>, IConvertToEavLight
{
    #region Constructor / DI

    public class MyServices: MyServicesBase
    {
        public LazySvc<IAppStates> AppStates { get; }
        public IValueConverter ValueConverter { get; }
        public IZoneCultureResolver ZoneCultureResolver { get; }

        public MyServices(LazySvc<IAppStates> appStates, IValueConverter valueConverter, IZoneCultureResolver zoneCultureResolver)
        {
            ConnectServices(
                AppStates = appStates,
                ValueConverter = valueConverter,
                ZoneCultureResolver = zoneCultureResolver
            );
        }
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

    /// <inheritdoc/>
    public void ConfigureForAdminUse()
    {
        WithGuid = true;
        WithPublishing = true;
        MetadataFor = new MetadataForSerialization { Serialize = true };
        Metadata = new SubEntitySerialization { Serialize = true, SerializeId = true, SerializeTitle = true, SerializeGuid = true };
        WithEditInfos = true;
    }

    #endregion

    #region Language

    public string[] Languages
    {
        get => _languages ??= Services.ZoneCultureResolver.SafeLanguagePriorityCodes();
        set => _languages = value;
    }

    [PrivateApi("not public ATM")]
    public TypeSerialization Type { get; set; } = new();

    private string[] _languages;

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
        // var rules = entity as IEntitySerialization;
        var rules = entity.GetDecorator<EntitySerializationDecorator>();
        var serRels = SubEntitySerialization.Stabilize(rules?.SerializeRelationships, true, false, true, false, true);

        // Exclude attributes when field type is Empty or attribute metadata is IsEphemeral
        // Check if the entity type is in the cache
        if (!_excludeAttributesCache.TryGetValue(entity.Type, out var excludeAttributes))
        {
            // If it's not in the cache, compute the list of attributes and add it to the cache
            excludeAttributes = entity.Type.Attributes?.ToList()
                .Where(a => a.Type == ValueTypes.Empty
                            || a.Metadata.GetBestValue<bool>(AttributeMetadata.MetadataFieldAllIsEphemeral))
                .Select(a => a.Name)
                .ToList();
            _excludeAttributesCache[entity.Type] = excludeAttributes;
        }

        // Convert Entity to dictionary
        // If the value is a relationship, then give those too, but only Title and Id
        var entityValues = entity.Attributes
            .Select(d => d.Value)
            .Where(d => excludeAttributes?.Contains(d.Name) != true)
            .ToEavLight(attribute => attribute.Name, attribute =>
            {
                var value = entity.GetBestValue(attribute.Name, Languages);

                // Special Case 1: Hyperlink Field which must be resolved
                if (attribute.Type == ValueTypes.Hyperlink && value is string stringValue &&
                    ValueConverterBase.CouldBeReference(stringValue))
                    return Services.ValueConverter.ToValue(stringValue, entity.EntityGuid);

                // Special Case 2: Entity-List
                if (attribute.Type == ValueTypes.Entity && value is IEnumerable<IEntity> entities)
                    return serRels.Serialize == true ? CreateListOrCsvOfSubEntities(entities, serRels) : null;

                // Default: Normal Value
                return value;
            });

        // todo: verify what happens with null-values on the relationships, maybe we should filter them out again?

        // New 12.05 - drop null values
        if(rules != null) OptimizeRemoveEmptyValues(rules, entityValues);

        AddAllIds(entity, entityValues, rules);

        if (WithPublishing)
        {
            var appState = Services.AppStates.Value.GetReader(entity.AppId);
            AddPublishingInformation(entity, entityValues, appState);
        };

        AddMetadataAndFor(entity, entityValues, rules);

        // Special edit infos - _Title (old, maybe not needed), Stats, EditInfo for read-only etc.
        if (WithEditInfos)
        {
            // this internal _Title field is probably not used much any more, so there is no rule for it
            // Probably remove at some time in near future, once verified it's not used in the admin-front-end
            try { entityValues.Add(InternalTitleField, entity.GetBestTitle(Languages)); }
            catch { /* ignore */ }

            AddStatistics(entity, entityValues);
            AddEditInfo(entity, entityValues);
        }


        // Include title field, if there is not already one in the dictionary
        if(rules?.SerializeTitle ?? true)
            if (!entityValues.ContainsKey(Attributes.TitleNiceName))
                entityValues.Add(Attributes.TitleNiceName, entity.GetBestTitle(Languages));
                
        AddDateInformation(entity, entityValues, rules);

        if (Type.Serialize) entityValues.Add(InternalTypeField, new JsonType(entity, Type.WithDescription));

        return entityValues;
    }
    private readonly Dictionary<object, List<string>> _excludeAttributesCache = new();

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