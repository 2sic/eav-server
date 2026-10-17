using ToSic.Eav.Context;
using ToSic.Eav.Data.EntityDecorators.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Serialization.Sys.Options;
using ToSic.Eav.WebApi.Sys.Entities;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.WebApi.Sys.Admin;

/// <summary>
/// Shared entity loading and serialization setup for admin data sources.
/// </summary>
[PrivateApi]
public class EntitiesAdminData(
    LazySvc<IContextOfSite> siteContext,
    LazySvc<IAppsCatalog> appsCatalog,
    LazySvc<EntityApi> entityApi)
{
    public IEnumerable<IEntity> GetEntities(int appId, string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return [];

        var entities = entityApi.Value
            .InitOrThrowBasedOnGrants(
                siteContext.Value,
                appsCatalog.Value.AppIdentity(appId),
                contentType!,
                GrantSets.ReadSomething)
            .GetEntitiesForAdminStep1(contentType!);

        return entities
            .Select(entity => new EntityWithDecorator<EntitySerializationDecorator>(entity, SerializationDecorator))
            .ToImmutableOpt();
    }

    // This matches ConvertToEavLight.ConfigureForAdminUse().
    private static readonly EntitySerializationDecorator SerializationDecorator = new()
    {
        SerializeGuid = true,
        WithPublishing = true,
        SerializeMetadataFor = new() { Serialize = true },
        SerializeMetadata = new SubEntitySerialization
        {
            Serialize = true,
            SerializeId = true,
            SerializeTitle = true,
            SerializeGuid = true,
        },
        WithEditInfos = true,
        LinksWithBothValues = true,
    };
}
