using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys;
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
    public EntityInspectRelationships(Dependencies services, IAppReaderFactory appReaders, ISysFeaturesService featuresSvc)
        : base(services, $"{DataSourceConstantsInternal.LogPrefix}.FState", connect: [appReaders, featuresSvc])
    {
        ProvideOutRaw(
            () => GetList(appReaders, featuresSvc),
            options: () => new() { TypeName = "EntityRelationship", AutoId = false }
        );
    }

    private IEnumerable<IRawEntity> GetList(IAppReaderFactory appReaders, ISysFeaturesService featuresSvc)
    {
        var id = Id;
        var l = Log.Fn<IEnumerable<IRawEntity>>($"Id: {id}");
        if (id == 0)
            return l.Return([], "no id provided, []");

        var appReader = appReaders.Get(this.PureIdentity());

        var entity = appReader.List.GetOne(Id);

        if (entity == null)
            return l.Return([], $"no entity with id {id} found, []");

        var featureEnabled = featuresSvc.IsEnabled(BuiltInFeatures.EntityInspectRelationships);


        var childrenWithField = entity.Attributes
            .GetEntityAttributes()
            .SelectMany(a => a.Value.TypedContents?
                .Select(e => new RelInfo(e, Field: a.Key, IsChild: true)))
            .ToList();

        var parentsWithField = entity.Relationships
            .FindParents(log: l)
            .SelectMany(parent => parent.Attributes
                .GetEntityAttributes()
                .Where(pAttribs => pAttribs.Value.TypedContents?.Any(child => child.EntityId == id) == true)
                .Select(a => new RelInfo(parent, Field: a.Key, IsChild: false))
            )
            .ToList();

        var merged = childrenWithField.Union(parentsWithField).ToList();

        var converted = merged.Select(m => m.ToRawEntity(featureEnabled));

        return converted;
    }

    private record RelInfo(IEntity Entity, string Field, bool IsChild)
    {
        public RawEntity ToRawEntity(bool featureEnabled) =>
            new()
            {
                Guid = featureEnabled ? Entity.EntityGuid : Guid.Empty,
                Id = featureEnabled ? Entity.EntityId : 0,
                Values = new Dictionary<string, object?>
                {
                    { AttributeNames.TitleNiceName, featureEnabled ? Entity.GetBestTitle() ?? "unknown" : FeatureNotEnabledMessage },
                    { "Field", Field },
                    { "IsChild", IsChild },
                    { "ContentTypeName", featureEnabled ? Entity.Type.Name : "must enable feature" },
                    { "ContentTypeNameId", featureEnabled ? Entity.Type.NameId : "must enable feature" }
                }
            };

        private static readonly string FeatureNotEnabledMessage =
            $"hidden, feature {BuiltInFeatures.EntityInspectRelationships.NameId} not enabled";
    }
}