using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.Data;

/// <summary>
/// Helper extensions for lists of entities.
/// </summary>
/// <remarks>
/// Has been used internally since forever, made public in v21.
/// </remarks>
[PublicApi]
public static partial class EntityListExtensions
{
#if DEBUG
    // ReSharper disable NotAccessedField.Local
    private static int _countOneId;
    private static int _countOneGuid;
    private static int _countOneHas;
    //// ReSharper restore NotAccessedField.Local
#endif

    /// <summary>
    /// Get an entity with an entity-id - or null if not found
    /// </summary>
    /// <param name="list"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static IEntity? One(this IEnumerable<IEntity> list, int id)
    {
#if DEBUG
        _countOneId++;
#endif
        return SysPerfSettings.CacheListAutoIndex && list is ImmutableSmartList fastList
            ? fastList.Fast.Get(id)
            : list.FirstOrDefault(e => e.EntityId == id);
    }

    /// <summary>
    /// get an entity based on the guid - or null if not found
    /// </summary>
    /// <param name="list"></param>
    /// <param name="guid"></param>
    /// <returns></returns>
    public static IEntity? One(this IEnumerable<IEntity> list, Guid guid)
    {
#if DEBUG
        _countOneGuid++;
#endif
        return SysPerfSettings.CacheListAutoIndex && list is ImmutableSmartList fastList
            ? fastList.Fast.Get(guid)
            : list.FirstOrDefault(e => e.EntityGuid == guid);
    }


    /// <summary>
    /// Check if an entity is available. 
    /// Mainly used in special cases where published/unpublished are hidden/visible
    /// </summary>
    /// <param name="list"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool Has(this IEnumerable<IEntity> list, int id)
    {
#if DEBUG
        _countOneHas++;
#endif
        return SysPerfSettings.CacheListAutoIndex && list is ImmutableSmartList fastList
            ? fastList.Fast.Has(id)
            : list.Any(e => e.EntityId == id || e.RepositoryId == id);
    }
}
