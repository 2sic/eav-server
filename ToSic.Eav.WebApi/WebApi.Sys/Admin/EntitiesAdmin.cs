using ToSic.Eav.Context;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.WebApi.Sys.Entities;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.WebApi.Sys.Admin;

[PrivateApi]
[VisualQuery(
    NiceName = "Entities Admin",
    NameId = "7dd4fe46-7a83-4cc6-b3d3-d506b335b290",
    NameIds = ["System.EntitiesAdmin"],
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.Internal,
    UiHint = "Admin list of entities for a content type"
)]
public class EntitiesAdmin : CustomDataSource
{
    private readonly LazySvc<IContextOfSite> _siteContext;
    private readonly LazySvc<IAppsCatalog> _appsCatalog;
    private readonly LazySvc<EntityApi> _entityApi;

    #region Configuration Properties

    /// <summary>
    /// The static name of the content type.
    /// </summary>
    [Configuration(Fallback = "")]
    public string ContentType => Configuration.GetThis(fallback: "");

    #endregion

    public EntitiesAdmin(
        Dependencies services,
        LazySvc<IContextOfSite> siteContext,
        LazySvc<IAppsCatalog> appsCatalog,
        LazySvc<EntityApi> entityApi)
        : base(services, logName: "Eav.EntitiesAdmin", connect: [siteContext, appsCatalog, entityApi])
    {
        _siteContext = siteContext;
        _appsCatalog = appsCatalog;
        _entityApi = entityApi;

        ProvideOutRaw(GetEntities, options: () => new()
        {
            TitleField = "Title",
            TypeName = "Entity",
            AllowUnknownValueTypes = true,
        });
    }

    private IEnumerable<IRawEntity> GetEntities()
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();

        if (string.IsNullOrWhiteSpace(ContentType))
            return l.Return([], "no content type");

        var app = _appsCatalog.Value.AppIdentity(AppId);

        var entities = _entityApi.Value
            .InitOrThrowBasedOnGrants(_siteContext.Value, app, ContentType, GrantSets.ReadSomething)
            .GetEntitiesForAdmin(ContentType)
            .Select(ToRawEntity)
            .ToList();

        return l.Return(entities, $"{entities.Count}");
    }

    private static RawEntity ToRawEntity(Dictionary<string, object> entity)
    {
        var now = DateTime.Now;
        return new()
        {
            Id = TryGetInt(entity, nameof(IRawEntity.Id)),
            Guid = TryGetGuid(entity, nameof(IRawEntity.Guid)),
            Created = TryGetDateTime(entity, nameof(IRawEntity.Created), now),
            Modified = TryGetDateTime(entity, nameof(IRawEntity.Modified), now),
            Values = entity.ToDictionary(pair => pair.Key, pair => (object?)pair.Value),
        };
    }

    private static int TryGetInt(IDictionary<string, object> values, string key)
        => values.TryGetValue(key, out var value) && int.TryParse(value?.ToString(), out var result)
            ? result
            : 0;

    private static Guid TryGetGuid(IDictionary<string, object> values, string key)
        => values.TryGetValue(key, out var value) && Guid.TryParse(value?.ToString(), out var result)
            ? result
            : Guid.Empty;

    private static DateTime TryGetDateTime(IDictionary<string, object> values, string key, DateTime fallback)
        => values.TryGetValue(key, out var value) && value is DateTime result
            ? result
            : fallback;
}