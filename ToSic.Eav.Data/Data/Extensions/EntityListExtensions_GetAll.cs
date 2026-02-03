using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.Data;

public static partial class EntityListExtensions
{
#if DEBUG
    // ReSharper disable NotAccessedField.Local
    private static int _countOneOfContentType;
    //// ReSharper restore NotAccessedField.Local
#endif

    // Todo #OptimizeOfType

    /// <summary>
    /// Extract all entities of a specific content type from a list.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IEnumerable<IEntity> GetAll(this IEnumerable<IEntity> list, IContentType type)
    {
        return SysPerfSettings.CacheListAutoIndex && list is ImmutableSmartList fastList
            ? fastList.GetAll(typeName: type.NameId)  // reuse existing functionality & index but using the most reliable nameId
            : list.Where(e => type.Equals(e.Type)).ToListOpt();
    }

    /// <summary>
    /// Extract all entities of a specific content type from a list.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static IEnumerable<IEntity> GetAll(this IEnumerable<IEntity> list, string typeName)
    {
#if DEBUG
        _countOneOfContentType++;
#endif            
        return SysPerfSettings.CacheListAutoIndex && list is ImmutableSmartList fastList
            ? fastList.Fast.OfType(typeName)
            : list
                .Where(e => e.Type.Is(typeName))
                .ToListOpt();
    }


}
