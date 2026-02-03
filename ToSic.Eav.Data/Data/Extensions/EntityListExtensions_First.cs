using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.Data;

public static partial class EntityListExtensions
{
    #region Non-Generic

    /// <summary>
    /// Returns the first entity that matches the specified type name, or null if not found.
    /// </summary>
    /// <param name="list">The collection of entities to search.</param>
    /// <param name="typeName">The name of the type to match. This comparison is case-sensitive.</param>
    /// <returns>The first entity whose type matches the specified type name, or null if no matching entity is found.</returns>
    public static IEntity? First(
        this IEnumerable<IEntity> list,
        string typeName
    )
    {
        return SysPerfSettings.CacheListAutoIndex && list is ImmutableSmartList fastList
            ? fastList.Fast.OfType(typeName).FirstOrDefault()
            : list.FirstOrDefault(e => e.Type.Is(typeName));
    }

    #endregion


}
