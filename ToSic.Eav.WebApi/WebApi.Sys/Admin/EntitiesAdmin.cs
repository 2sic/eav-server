using ToSic.Eav.Context;
using ToSic.Eav.Data.EntityDecorators.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.Serialization.Sys.Options;
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

        ProvideOut(GetEntities);
    }

    private IEnumerable<IEntity> GetEntities()
    {
        var l = Log.Fn<IEnumerable<IEntity>>();

        if (string.IsNullOrWhiteSpace(ContentType))
            return l.Return([], "no content type");

        var app = _appsCatalog.Value.AppIdentity(AppId);

        var entities = _entityApi.Value
            .InitOrThrowBasedOnGrants(_context.Value, app, ContentType, GrantSets.ReadSomething)
            .GetEntitiesForAdminStep1(ContentType);

        // Attach serializationMetadata
        // This matches the ConvertToEavLight.ConfigureForAdminUse()
        var decorator = new EntitySerializationDecorator
        {
            SerializeGuid = true,
            WithPublishing = true,
            SerializeMetadataFor = new() { Serialize = true },
            SerializeMetadata = new SubEntitySerialization
            {
                Serialize = true,
                SerializeId = true,
                SerializeTitle = true,
                SerializeGuid = true
            },
            WithEditInfos = true,
            LinksWithBothValues = true,
        };

        var result = entities
            .Select(IEntity (e) => new EntityWithDecorator<EntitySerializationDecorator>(e, decorator))
            .ToImmutableOpt();


        return l.Return(result);
    }
}