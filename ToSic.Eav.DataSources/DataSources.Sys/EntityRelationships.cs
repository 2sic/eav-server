using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.DataSource.Sys;

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
    NiceName = "Entity Relationships",
    UiHint = "List all entity relationships",
    Type = DataSourceType.System,
    NameId = "4f5faacb-27bd-4946-ae41-9fe46f9f260c",
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.System
)]
// ReSharper disable once UnusedMember.Global
public sealed class EntityRelationships : CustomDataSource
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
    public EntityRelationships(Dependencies services, IAppReaderFactory appReaders) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.FState", connect: [appReaders])
    {
        ProvideOutRaw(
            () => GetList(appReaders),
            options: () => new() { TypeName = "EntityRelationships" }
        );
    }

    private IEnumerable<IRawEntity> GetList(IAppReaderFactory appReaders)
    {
        var id = Id;
        var l = Log.Fn<IEnumerable<IRawEntity>>($"Id: {id}");
        if (id == 0)
            return l.Return([], "no id provided, []");

        var appReader = appReaders.Get(this.PureIdentity());

        var entity = appReader.List.GetOne(Id);

        if (entity == null)
            return l.Return([], $"no entity with id {id} found, []");


        var childrenWithField = entity.Attributes
            .GetEntityAttributes()
            .SelectMany(a => a.Value.TypedContents?
                .Select(e => new RelInfo(e)
                {
                    IsChild = true,
                    Field = a.Key,
                }) ?? [])
            .ToList();

        var parentsWithField = entity.Relationships
            .FindParents(log: l)
            .SelectMany(parent => parent.Attributes
                .GetEntityAttributes()
                .Where(parentAttributes => parentAttributes.Value
                    .TypedContents?.Any(child => child.EntityId == id) == true)
                .Select(a => new RelInfo(parent)
                {
                    IsChild = false,
                    Field = a.Key,
                }))
            .ToList();

        var merged = childrenWithField.Union(parentsWithField).ToList();

        var converted = merged.Select(m => m.ToRawEntity());
        //    new RawEntity()
        //{
        //    Guid = m.Guid,
        //    Id = m.Id,
        //    Values = new Dictionary<string, object?>()
        //    {
        //        { AttributeNames.TitleNiceName, m.Title },
        //        { "Field", m.Field },
        //        { "IsChild", m.IsChild }
        //    }
        //});

        return converted;
    }

    private record RelInfo(IEntity Entity)
    {
        public required string Field { get; init; }

        public bool IsChild { get; init; }

        //public int Id => Entity.EntityId;

        //public Guid Guid => Entity.EntityGuid;

        //public string Title => Entity.GetBestTitle() ?? "unknown";

        public RawEntity ToRawEntity() =>
            new()
            {
                Guid = Entity.EntityGuid,
                Id = Entity.EntityId,
                Values = new Dictionary<string, object?>()
                {
                    { AttributeNames.TitleNiceName, Entity.GetBestTitle() ?? "unknown" },
                    { "Field", Field },
                    { "IsChild", IsChild }
                }
            };
    }
}