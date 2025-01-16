using ToSic.Eav.Plumbing;
using ToSic.Eav.Serialization;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// DataSource which changes how Streams will be serialized in the end.
/// </summary>
/// <remarks>
/// * New in v11.20
/// * Changed in v15.05 to use the [immutable convention](xref:NetCode.Conventions.Immutable)
/// * Renamed to `SerializationConfiguration` from `SerializationConfiguration` in 15.05
/// </remarks>
[PublicApi]
[VisualQuery(
    NiceName = "Serialization Configuration",
    UiHint = "Determine how this data is Serialized",
    Icon = DataSourceIcons.HtmlDotDotDot,
    Type = DataSourceType.Modify, 
    NameId = "2952e680-4aaa-4a12-adf7-325cb2854358",
    DynamicOut = true,
    In = [DataSourceConstants.StreamDefaultName],
    ConfigurationType = "5c84cd3f-f853-40b3-81cf-dee6a07dc411",
    HelpLink = "https://go.2sxc.org/DsSerializationConfiguration")]

public partial class Serialization : DataSourceBase
{
    #region Constants

    [PrivateApi("not sure how to document, doesn't seem to be in use")] internal static string KeepAll = "*";
    [PrivateApi("not sure how to document, doesn't seem to be in use")] internal static string KeepNone = "-";

    #endregion
    #region Configuration-properties
    private const string RmvEmptyStringsKey = "RemoveEmptyStringValues";
    private const string RmvBooleanFalseKey = "RemoveFalseValues";

    #region Basic Fields

    /// <summary>
    /// Should the ID be included in serialization
    /// </summary>
    [Configuration]
    public string IncludeId => Configuration.GetThis();

    /// <summary>
    /// Should the AppId be included in serialization.
    /// Especially for scenarios where data is retrieved from multiple Apps
    /// </summary>
    [Configuration]
    public string IncludeAppId => Configuration.GetThis();

    /// <summary>
    /// Should the AppId be included in serialization.
    /// Especially for scenarios where data is retrieved from multiple Apps
    /// </summary>
    [Configuration]
    public string IncludeZoneId => Configuration.GetThis();

    /// <summary>
    /// Should the GUID be included in serialization
    /// </summary>
    [Configuration]
    public string IncludeGuid => Configuration.GetThis();

    /// <summary>
    /// Should the default Title be included as "Title" in serialization
    /// </summary>
    [Configuration]
    public string IncludeTitle => Configuration.GetThis();

    #endregion

    #region Dates

    /// <summary>
    /// Should the Modified date be included in serialization
    /// </summary>
    [Configuration]
    public string IncludeModified => Configuration.GetThis();

    /// <summary>
    /// Should the Created date be included in serialization
    /// </summary>
    [Configuration]
    public string IncludeCreated => Configuration.GetThis();

    #endregion

    #region Optimize - new in 12.05

    /// <summary>
    /// todo
    /// </summary>
    [Configuration]
    public string RemoveNullValues => Configuration.GetThis();

    /// <summary>
    /// todo
    /// </summary>
    [Configuration]
    public string RemoveZeroValues => Configuration.GetThis();

    /// <summary>
    /// todo
    /// </summary>
    [Configuration(Field = RmvEmptyStringsKey)]
    public string RemoveEmptyStrings => Configuration.GetThis();

    /// <summary>
    /// todo
    /// </summary>
    [Configuration(Field = RmvBooleanFalseKey)]
    public string DropFalseValues => Configuration.GetThis();

    #endregion

    #region Metadata For - enhanced in 12.05

    /// <summary>
    /// Should the Metadata target/for information be included in serialization
    /// </summary>
    [Configuration]
    public string IncludeMetadataFor => Configuration.GetThis();

    /// <summary>
    /// Should the Metadata target/for information be included in serialization
    /// </summary>
    [Configuration]
    public string IncludeMetadataForId => Configuration.GetThis();

    /// <summary>
    /// Should the Metadata target/for information be included in serialization
    /// </summary>
    [Configuration]
    public string IncludeMetadataForType => Configuration.GetThis();
    #endregion

    #region Metadata

    /// <summary>
    /// Should the Metadata ID be included in serialization
    /// </summary>
    [Configuration]
    public string IncludeMetadata => Configuration.GetThis();

    /// <summary>
    /// Should the Metadata ID be included in serialization
    /// </summary>
    [Configuration]
    public string IncludeMetadataId => Configuration.GetThis();

    /// <summary>
    /// Should the Metadata GUID be included in serialization
    /// </summary>
    [Configuration]
    public string IncludeMetadataGuid => Configuration.GetThis();

    /// <summary>
    /// Should the default Title of the Metadata be included as "Title" in serialization
    /// </summary>
    [Configuration]
    public string IncludeMetadataTitle => Configuration.GetThis();

    #endregion

    #region Relationships


    /// <summary>
    /// Should the Relationships be included in serialization
    /// </summary>
    [Configuration]
    public string IncludeRelationships => Configuration.GetThis();

    /// <summary>
    /// Should the Relationships be included as CSV like "42,27,999".
    /// Possible values
    /// * null or false: they are sub-objects
    /// * true or "csv": they are CSV strings
    /// * "array" return array of ID or GUID
    /// </summary>
    /// <remarks>
    /// * adding in v15.03
    /// * extended purpose in v18.00 to also have "array" as possible value
    /// </remarks>
    [Configuration(Fallback = false)]
    public string IncludeRelationshipsAsCsv => Configuration.GetThis();

    /// <summary>
    /// Should the Relationship ID be included in serialization
    /// </summary>
    [Configuration]
    public string IncludeRelationshipId => Configuration.GetThis();

    /// <summary>
    /// Should the Relationship GUID be included in serialization
    /// </summary>
    [Configuration]
    public string IncludeRelationshipGuid => Configuration.GetThis();

    /// <summary>
    /// Should the default Title of the Relationship be included as "Title" in serialization
    /// </summary>
    [Configuration]
    public string IncludeRelationshipTitle => Configuration.GetThis();
    #endregion

    #region ContentType Information

    /// <summary>
    /// Values probably
    /// - empty (default) - don't include
    /// - "object" make an object
    /// - "flat"
    /// </summary>
    [Configuration]
    public string IncludeTypeAs => Configuration.GetThis();

    [Configuration(Fallback = "Type")]
    public string TypePropertyNames => Configuration.GetThis();

    [Configuration]
    public string IncludeTypeId => Configuration.GetThis();

    [Configuration]
    public string IncludeTypeName => Configuration.GetThis();

    #endregion

    #endregion


    /// <inheritdoc />
    /// <summary>
    /// Constructs a new AttributeFilter DataSource
    /// </summary>
    [PrivateApi]
    public Serialization(MyServices services) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.SerCnf")
    {
    }

    /// <summary>
    /// Get the list of all items with reduced attributes-list
    /// </summary>
    /// <returns></returns>
    private IImmutableList<IEntity> GetList(string inStreamName = DataSourceConstants.StreamDefaultName)
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        Configuration.Parse();
        var original = In[inStreamName].List.ToImmutableList();
        var enhanced = AddSerializationRules(original);
        return l.Return(enhanced, $"{enhanced.Count}");
    }

    private IImmutableList<IEntity> AddSerializationRules(IImmutableList<IEntity> before) => Log.Func(() =>
    {
        // Skip if no rules defined
        var noRules = string.IsNullOrWhiteSpace(string.Join("", Configuration));
        if (noRules) return (before, "no rules, unmodified");

        var decorator = Decorator;

        var result = before
            .Select(e => (IEntity)new EntityDecorator12<EntitySerializationDecorator>(e, decorator));

        return (result.ToImmutableList(), "modified");
    });

    private EntitySerializationDecorator Decorator => _decorator ??= CreateDecorator();
    private EntitySerializationDecorator _decorator;

    private EntitySerializationDecorator CreateDecorator()
    {
        var id = TryParseIncludeRule(IncludeId);
        var title = TryParseIncludeRule(IncludeTitle);
        var guid = TryParseIncludeRule(IncludeGuid);
        var created = TryParseIncludeRule(IncludeCreated);
        var modified = TryParseIncludeRule(IncludeModified);
        var appId = TryParseIncludeRule(IncludeAppId);
        var zoneId = TryParseIncludeRule(IncludeZoneId);

        var dropNullValues = TryParseIncludeRule(RemoveNullValues) ?? false;
        var dropZeroValues = TryParseIncludeRule(RemoveZeroValues) ?? false;
        var dropEmptyStringValues = TryParseIncludeRule(RemoveEmptyStrings) ?? false;
        var dropFalseValues = TryParseIncludeRule(DropFalseValues) ?? false;

        var mdForSer = new MetadataForSerialization
        {
            Serialize = TryParseIncludeRule(IncludeMetadataFor),
            SerializeKey = TryParseIncludeRule(IncludeMetadataForId),
            SerializeType = TryParseIncludeRule(IncludeMetadataForType),
        };

        var mdSer = new SubEntitySerialization
        {
            Serialize = TryParseIncludeRule(IncludeMetadata),
            SerializeId = TryParseIncludeRule(IncludeMetadataId),
            SerializeGuid = TryParseIncludeRule(IncludeMetadataGuid),
            SerializeTitle = TryParseIncludeRule(IncludeMetadataTitle)
        };


        var relSer = new SubEntitySerialization
        {
            Serialize = TryParseIncludeRule(IncludeRelationships),
            // Serialize as CSV can be null, false, true, "array"
            //SerializesAsCsv = TryParseIncludeRule(IncludeRelationshipsAsCsv) ?? IncludeRelationshipsAsCsv.HasValue(),
            //SerializeListAsString = !IncludeRelationshipsAsCsv.EqualsInsensitive("array"),
            SerializeFormat = GetOutputFormat(),

            SerializeId = TryParseIncludeRule(IncludeRelationshipId),
            SerializeGuid = TryParseIncludeRule(IncludeRelationshipGuid),
            SerializeTitle = TryParseIncludeRule(IncludeRelationshipTitle)
        };

        var typeSer = new TypeSerialization
        {
            SerializeAs = IncludeTypeAs,
            SerializeId = TryParseIncludeRule(IncludeTypeId),
            SerializeName = TryParseIncludeRule(IncludeTypeName),
            SerializeDescription = null,
            PropertyNames = TypePropertyNames
        };

        var decorator = new EntitySerializationDecorator
        {
            RemoveNullValues = dropNullValues,
            RemoveZeroValues = dropZeroValues,
            RemoveEmptyStringValues = dropEmptyStringValues,
            RemoveBoolFalseValues = dropFalseValues,

            // Metadata & Relationships
            SerializeMetadataFor = mdForSer,
            SerializeMetadata = mdSer,
            SerializeRelationships = relSer,
            SerializeType = typeSer,

            // id, title, guid
            SerializeId = id,
            SerializeTitle = title,
            SerializeGuid = guid,
            SerializeAppId = appId,
            SerializeZoneId = zoneId,

            // dates
            SerializeCreated = created,
            SerializeModified = modified
        };
        return decorator;
    }

    private string GetOutputFormat()
    {
        if (IncludeRelationshipsAsCsv.IsEmptyOrWs()) return null;
        var csvAsBool = TryParseIncludeRule(IncludeRelationshipsAsCsv);
        if (csvAsBool == true) return "csv";
        if (csvAsBool == false) return null;
        return IncludeRelationshipsAsCsv; // could be "array"
    }

    private bool? TryParseIncludeRule(string original)
        => bool.TryParse(original, out var include) ? (bool?)include : null;

}