using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data.Entities.Sys.Lists;

[PrivateApi]
// ReSharper disable once InconsistentNaming
public static class IEntityExtensions
{
#if DEBUG
    private static int _countOneId;
    private static int _countOneGuid;
    private static int _countOneHas;
    private static int _countOneRepo;
    private static int _countOneOfContentType;

    internal static int CountOneIdOpt;
    internal static int CountOneRepoOpt;
    internal static int CountOneHasOpt;
    internal static int CountOneGuidOpt;
    internal static int CountOneOfContentTypeOpt;
#endif
    /// <summary>
    /// Get an entity with an entity-id - or null if not found
    /// </summary>
    /// <param name="list"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IEntity One(this IEnumerable<IEntity> list, int id)
    {
#if DEBUG
        _countOneId++;
#endif
        return SysPerfSettings.EnableLazyFastAccess && list is ImmutableSmartList fastList
            ? fastList.Fast.Get(id)
            : list.FirstOrDefault(e => e.EntityId == id);
    }

    /// <summary>
    /// get an entity based on the guid - or null if not found
    /// </summary>
    /// <param name="list"></param>
    /// <param name="guid"></param>
    /// <returns></returns>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IEntity One(this IEnumerable<IEntity> list, Guid guid)
    {
#if DEBUG
        _countOneGuid++;
#endif
        return SysPerfSettings.EnableLazyFastAccess && list is ImmutableSmartList fastList
            ? fastList.Fast.Get(guid)
            : list.FirstOrDefault(e => e.EntityGuid == guid);
    }


    /// <summary>
    /// Get an entity based on the repo-id - mainly used for internal APIs and saving/versioning
    /// </summary>
    /// <param name="list"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IEntity FindRepoId(this IEnumerable<IEntity> list, int id)
    {
#if DEBUG
        _countOneRepo++;
#endif
        return SysPerfSettings.EnableLazyFastAccess && list is ImmutableSmartList fastList
            ? fastList.Fast.GetRepo(id)
            : list.FirstOrDefault(e => e.RepositoryId == id);
    }

    /// <summary>
    /// Check if an entity is available. 
    /// Mainly used in special cases where published/unpublished are hidden/visible
    /// </summary>
    /// <param name="list"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool Has(this IEnumerable<IEntity> list, int id)
    {
#if DEBUG
        _countOneHas++;
#endif
        return SysPerfSettings.EnableLazyFastAccess && list is ImmutableSmartList fastList
            ? fastList.Fast.Has(id)
            : list.Any(e => e.EntityId == id || e.RepositoryId == id);
    }


    // Todo #OptimizeOfType

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IEnumerable<IEntity> OfType(this IEnumerable<IEntity> list, IContentType type)
        => list.Where(e => type.Equals(e.Type));

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IEnumerable<IEntity> OfType(this IEnumerable<IEntity> list, string typeName)
    {
#if DEBUG
        _countOneOfContentType++;
#endif            
        return SysPerfSettings.EnableLazyFastAccess && list is ImmutableSmartList fastList
            ? fastList.Fast.OfType(typeName)
            : list
                .Where(e => e.Type.Is(typeName))
                .ToListOpt();
    }

    public static IEntity IfOfType(this IEntity entity, string typeName) 
        => entity.Type.Is(typeName) ? entity : null;


    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static Dictionary<string, object> AsDictionary(this IEntity entity)
    {
        var attributes = entity.Attributes.ToDictionary(
            k => k.Value.Name,
            v => v.Value[0]
        );
        attributes.Add(AttributeNames.EntityIdPascalCase, entity.EntityId);
        attributes.Add(AttributeNames.EntityGuidPascalCase, entity.EntityGuid);
        return attributes;
    }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IEntity KeepOrThrowIfInvalid(this IEntity item, string contentType, object identifier)
    {
        if (item == null || contentType != null && !item.Type.Is(contentType))
            throw new KeyNotFoundException($"Can't find {identifier} of type '{contentType}'");
        return item;
    }
}