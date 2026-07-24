using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.DataSource.Sys;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.DataSources.Sys;

/// <summary>
/// A DataSource that list all entity relationships.
/// </summary>
/// <remarks>
/// New v21.02
/// </remarks>
/// <inheritdoc />
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
[VisualQuery(
    NiceName = "Entity inspect Relationships",
    UiHint = "List all entity relationships",
    Type = DataSourceType.System,
    NameId = "4f5faacb-27bd-4946-ae41-9fe46f9f260c",
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.System
)]
// ReSharper disable once UnusedMember.Global
public sealed class EntityInspectRelationships : CustomDataSource
{
    /// <summary>
    /// Required filter to only return specific features by their NameId, comma-separated. E.g. "Feature1,Feature2"
    /// </summary>
    /// <remarks>
    /// If blank or not set, will return all feature states.
    /// 
    /// Added in v21.02
    /// </remarks>
    [Configuration(Fallback = 0)]
    public int Id => Configuration.GetThis(0);

    [PrivateApi]
    public EntityInspectRelationships(Dependencies services, IAppReaderFactory appReaders, FeaturesForDataSources featuresForDs)
        : base(services, $"{DataSourceConstantsInternal.LogPrefix}.FState", connect: [appReaders, featuresForDs])
    {
        // Main stream
        ProvideOutRaw(
            () => GetRelationships(appReaders, featuresForDs.Features)//,
            // WIP - ideally adding the type would not be necessary, but ATM not yet perfect
            //options: () => new() { Type = typeof(EntityRelationship), AutoId = false }
        );

        // Feature State / Status
        ProvideOut(name: FeaturesForDataSources.StreamName,
            data: () => featuresForDs.GetDataForFeature(BuiltInFeatures.EntityInspectRelationships));

    }

    private IEnumerable<IRawEntity> GetRelationships(IAppReaderFactory appReaders, ISysFeaturesService featuresSvc)
    {
        var id = Id;
        var l = Log.Fn<IEnumerable<IRawEntity>>($"Id: {id}");
        if (id == 0)
            return l.Return([], "no id provided, []");

        // Check if Entity found
        var entity = appReaders.Get(this.PureIdentity()).List.GetOne(id);
        if (entity == null)
            return l.Return([], $"no entity with id {id} found, []");

        // Check if the feature is on, this changes what the user will see
        var featureEnabled = featuresSvc.IsEnabled(BuiltInFeatures.EntityInspectRelationships);

        // Get all the child relationships, incl. what field the data is in
        var childrenWithField = entity.Attributes
            .GetEntityAttributes()
            .SelectMany(a => a.Value.TypedContents?
                .Select(e => new EntityRelationship(e, Field: a.Key, IsChild: true, FeatEnabled: featureEnabled)) ?? [])
            .ToList();

        // Get all the parent relationships, incl. what field the data is in
        var parentsWithField = entity.Relationships
            .FindParents(log: l)
            .SelectMany(parent => parent.Attributes
                .GetEntityAttributes()
                .Where(pAttribs => pAttribs.Value.TypedContents?.Any(child => child.EntityId == id) == true)
                .Select(a => new EntityRelationship(parent, Field: a.Key, IsChild: false, FeatEnabled: featureEnabled))
            )
            .ToList();

        // Merge, convert and return
        var merged = childrenWithField
            .Union(parentsWithField)
            .Cast<IRawEntity>()
            .ToList();

        return l.Return(merged);
    }

    [ContentTypeSpecs(
        Guid = "9878be6e-93d9-4d91-82a3-31ca4da436c3",
        Description = "Entity Relationship",
        Name = MyContentTypeName
    )]
    private record EntityRelationship(
        [property: ContentTypeAttributeIgnore]
        IEntity Entity, 
        string Field,
        bool IsChild,
        [property: ContentTypeAttributeIgnore]
        bool FeatEnabled
    ) : /*RawEntityRecordBase,*/ IRawEntityConvertible
    {
        private const string MyContentTypeName = "EntityRelationship";

        public int Id => FeatEnabled ? Entity.EntityId : 0;
        
        public Guid Guid => FeatEnabled ? Entity.EntityGuid : Guid.Empty;

        public string Title => FeatEnabled ? Entity.GetBestTitle() ?? "unknown" : FeatureNotEnabledMessage;
        
        public string ContentTypeName => FeatEnabled ? Entity.Type.Name : MustEnableFeature;
        
        public string ContentTypeNameId => FeatEnabled ? Entity.Type.NameId : MustEnableFeature;

        //public override IDictionary<string, object?> Values => new Dictionary<string, object?>
        //    {
        //        { AttributeNames.TitleNiceName, Title },
        //        { nameof(Field), Field },
        //        { nameof(IsChild), IsChild },
        //        { nameof(ContentTypeName), ContentTypeName },
        //        { nameof(ContentTypeNameId), ContentTypeNameId }
        //    };

        //public override IDictionary<string, object?> Attributes(RawConvertOptions options) => Values;
            
        
        private const string MustEnableFeature = "must enable feature";
        private static readonly string FeatureNotEnabledMessage =
            $"hidden, feature {BuiltInFeatures.EntityInspectRelationships.NameId} not enabled";

        IRawEntityConverter IRawEntityConvertible.GetConverter() => Converter;

        private static IRawEntityConverter Converter { get; } =
            new RawEntityConverterFactory<EntityRelationship>((source, _) =>
                new RawEntityRecord
                {
                    Id = source.Id,
                    Guid = source.Guid,
                    Values = new Dictionary<string, object?>
                    {
                        { AttributeNames.TitleNiceName, source.Title },
                        { nameof(Field), source.Field },
                        { nameof(IsChild), source.IsChild },
                        { nameof(ContentTypeName), source.ContentTypeName },
                        { nameof(ContentTypeNameId), source.ContentTypeNameId }
                    },
                });

    }
}