using System.Collections.ObjectModel;
using ToSic.Eav.DataSource.Streams.Internal;
using ToSic.Eav.Serialization;
using ToSic.Lib.Helpers;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// DataSource which changes how Streams will be serialized in the end.
/// </summary>
/// <remarks>
/// * v11.20 Created
/// * v15.05 changed to use the [immutable convention](xref:NetCode.Conventions.Immutable)
/// * v15.05 Renamed to `SerializationConfiguration` from `SerializationConfiguration`
/// </remarks>
[PublicApi]
[VisualQuery(
    NiceName = "Serialization Configuration",
    UiHint = "Determine how this data is Serialized",
    Icon = DataSourceIcons.HtmlDotDotDot,
    Type = DataSourceType.Modify, 
    NameId = "2952e680-4aaa-4a12-adf7-325cb2854358",
    DynamicOut = true,
    OutMode = VisualQueryAttribute.OutModeMirrorIn, // New v20 - improved visual query
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
    { }

    #region Dynamic Out

    /// <inheritdoc/>
    public override IReadOnlyDictionary<string, IDataStream> Out => _getOut.Get(() => new ReadOnlyDictionary<string, IDataStream>(CreateOutWithAllStreams()));

    private readonly GetOnce<IReadOnlyDictionary<string, IDataStream>> _getOut = new();

    /// <summary>
    /// Attach all missing streams, now that Out is used the first time.
    /// </summary>
    private IDictionary<string, IDataStream> CreateOutWithAllStreams()
    {
        var outDic = new Dictionary<string, IDataStream>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var dataStream in In.Where(s => !outDic.ContainsKey(s.Key)))
            outDic.Add(dataStream.Key, new DataStream(Services.CacheService, this, dataStream.Key, () => GetList(dataStream.Key)));
        return outDic;
    }

    #endregion

    #region Generate Lists with Serialization Decorators

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

    private IImmutableList<IEntity> AddSerializationRules(IImmutableList<IEntity> before)
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        // Skip if no rules defined
        var noRules = string.IsNullOrWhiteSpace(string.Join("", Configuration));
        if (noRules)
            return l.Return(before, "no rules, unmodified");

        var decorator = SerializationSourceToDecorator.Create(this);

        var result = before
            .Select(IEntity (e) => new EntityDecorator12<EntitySerializationDecorator>(e, decorator))
            .ToImmutableList();

        return l.Return(result, "modified");
    }

    #endregion

}