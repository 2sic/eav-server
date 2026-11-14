using ToSic.Eav.Context;
using ToSic.Eav.Data.EntityDecorators.Sys;
using ToSic.Eav.Data.Sys.Ancestors;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Serialization.Sys.Options;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Data.Sys;

/// <summary>
/// Shared entity loading and serialization setup for admin data sources.
/// </summary>
[PrivateApi]
public class EntitiesAdminData(IContextOfSiteService ctxSvc, AppDataChain<WorkEntities> workEntities, AppDataChain<WorkTypePermissions> typePermissions)
    : ServiceWithSetup<IAppDataContext>("Wrk.EntAdm", connect: [ctxSvc, workEntities, typePermissions])
{
    public IEnumerable<IEntity> GetEntities(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return [];

        typePermissions.New(MyOptions).ThrowIfNotAllowed(contentType, GrantSets.ReadSomething);

        return GetEntitiesForAdminStep(contentType!)
            .Select(entity => new EntityWithDecorator<EntitySerializationDecorator>(entity, SerializationDecorator))
            .ToImmutableOpt();
    }


    private List<IEntity> GetEntitiesForAdminStep(string contentType, bool excludeAncestor = false)
    {
        var l = Log.Fn<List<IEntity>>(timer: true);

        var ofType = workEntities.New(MyOptions)
            .Get(contentType)
            .ToList();

        // in the successor app, we can get an additional AppConfiguration, AppSettings or AppResources from the ancestor app
        // that we can optionally exclude from the results
        var afterAncestorFilter = excludeAncestor
            ? ofType
                .Where(e => !e.HasAncestor())
                .ToList()
            : ofType;

        return l.Return(afterAncestorFilter, afterAncestorFilter.Count.ToString());
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