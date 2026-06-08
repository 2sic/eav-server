using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Context;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.WebApi.Sys.Entities;
using ToSic.Sys.Security.Permissions;
using static ToSic.Eav.DataSource.CustomDataSource;

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
    private readonly LazySvc<IContextOfSite> _context;
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
        LazySvc<IContextOfSite> context,
        LazySvc<IAppsCatalog> appsCatalog,
        LazySvc<EntityApi> entityApi)
        : base(services, logName: "Eav.EntitiesAdmin", connect: [context, appsCatalog, entityApi])
    {
        _context = context;
        _appsCatalog = appsCatalog;
        _entityApi = entityApi;

        ProvideOutRaw(GetEntities, options: () => new()
        {
            TitleField = "Title",
            TypeName = "Entity",
        });
    }

    private IEnumerable<IRawEntity> GetEntities()
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();

        if (string.IsNullOrWhiteSpace(ContentType))
            return l.Return([], "no content type");

        var app = _appsCatalog.Value.AppIdentity(AppId);

        var entities = _entityApi.Value
            .InitOrThrowBasedOnGrants(_context.Value, app, ContentType, GrantSets.ReadSomething)
            .GetEntitiesForAdmin(ContentType)
            .Select(entity => new RawEntity(entity))
            .ToList();

        return l.Return(entities, $"{entities.Count}");
    }
}