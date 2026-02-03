namespace ToSic.Eav.Data.Sys.Entities;

[PrivateApi]
// ReSharper disable once InconsistentNaming
public static class IEntityExtensions
{
#if DEBUG
    // ReSharper disable NotAccessedField.Local
    private static int _countOneRepo;
    private static int _countOneOfContentType;
    // ReSharper restore NotAccessedField.Local

    internal static int CountOneIdOpt;
    internal static int CountOneRepoOpt;
    internal static int CountOneHasOpt;
    internal static int CountOneGuidOpt;
    internal static int CountOneOfContentTypeOpt;
#endif


    /// <summary>
    /// Get an entity based on the repo-id - mainly used for internal APIs and saving/versioning
    /// </summary>
    /// <param name="list"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IEntity? FindRepoId(this IEnumerable<IEntity> list, int id)
    {
#if DEBUG
        _countOneRepo++;
#endif
        return SysPerfSettings.CacheListAutoIndex && list is ImmutableSmartList fastList
            ? fastList.Fast.GetRepo(id)
            : list.FirstOrDefault(e => e.RepositoryId == id);
    }

    // Experimental, only used in performance tests
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IEnumerable<IEntity> OfTypeCol(this IEnumerable<IEntity> list, string typeName)
    {
#if DEBUG
        _countOneOfContentType++;
#endif            
        return SysPerfSettings.CacheListAutoIndex && list is ImmutableSmartList fastList
            ? fastList.Fast.OfTypeCol(typeName)
            : list
                .Where(e => e.Type.Is(typeName))
                .ToListOpt();
    }

    public static IEntity? IfOfType(this IEntity entity, string typeName) 
        => entity.Type.Is(typeName) ? entity : null;


    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static Dictionary<string, object?> AsDictionary(this IEntity entity)
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
    public static IEntity KeepOrThrowIfInvalid(this IEntity? item, string? contentType, object identifier)
    {
        if (item == null || contentType != null && !item.Type.Is(contentType))
            throw new KeyNotFoundException($"Can't find {identifier} of type '{contentType}'");
        return item;
    }
}