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
    public static IEnumerable<IEntity> AllOfType(this IEnumerable<IEntity> list, IContentType type)
    {
        return SysPerfSettings.CacheListAutoIndex && list is ImmutableSmartList fastList
            ? fastList.AllOfType(type.NameId)  // reuse existing functionality & index but using the most reliable nameId
            : list.Where(e => type.Equals(e.Type)).ToListOpt();
    }

    /// <summary>
    /// Extract all entities of a specific content type from a list.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="typeName"></param>
    /// <returns></returns>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IEnumerable<IEntity> AllOfType(this IEnumerable<IEntity> list, string typeName)
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

    /// <summary>
    /// Returns a collection of wrapper objects of type `TModel` for all entities of the specified type.
    /// </summary>
    /// <typeparam name="TModel">
    /// The model type to wrap each entity.
    /// Must implement `IWrapperSetup{IEntity}` and have a parameterless constructor.
    /// </typeparam>
    /// <param name="md">The collection of entities to filter and wrap.</param>
    /// <returns>An enumerable collection of wrapped entities of the specified model type. Returns an empty collection if the
    /// input is null or contains no matching entities.</returns>
    public static IEnumerable<TModel> AllOfType<TModel>(this IEnumerable<IEntity>? md)
        where TModel : IWrapperSetup<IEntity>, new()
        => md.AllOfType<TModel>(typeof(TModel).Name);

    /// <summary>
    /// Returns a collection of wrapper objects of type `TModel` for all entities of the specified type name.
    /// </summary>
    /// <typeparam name="TModel">
    /// The model type to wrap each entity.
    /// Must implement `IWrapperSetup{IEntity}` and have a parameterless constructor.
    /// </typeparam>
    /// <param name="md">The source collection of entities to search. Can be null.</param>
    /// <param name="nameId">The name identifier of the entity type to filter by. This value is used to select entities of a specific type.</param>
    /// <returns>An enumerable collection of TModel instances wrapping the matching entities. Returns an empty collection if the
    /// source is null or no matching entities are found.</returns>
    public static IEnumerable<TModel> AllOfType<TModel>(this IEnumerable<IEntity>? md, string nameId)
        where TModel : IWrapperSetup<IEntity>, new()
    {
        if (md == null)
            return [];

        var list = md
            .AllOfType(nameId)
            .Where(x => x != null)
            .ToList();

        var result = list
            .Where(r => r != null)
            .Select(raw => raw.As<TModel>()!)
            .ToList();

        return result;
    }


    /// <summary>
    /// Returns the first entity that matches the specified type name, or null if not found.
    /// </summary>
    /// <param name="list">The collection of entities to search.</param>
    /// <param name="typeName">The name of the type to match. This comparison is case-sensitive.</param>
    /// <returns>The first entity whose type matches the specified type name, or null if no matching entity is found.</returns>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IEntity? OneOfType(this IEnumerable<IEntity> list, string typeName)
    {
        return SysPerfSettings.CacheListAutoIndex && list is ImmutableSmartList fastList
            ? fastList.Fast.OfType(typeName).FirstOrDefault()
            : list.FirstOrDefault(e => e.Type.Is(typeName));
    }

    /// <summary>
    /// Returns the first entity that matches the specified type name, or null if not found.
    /// </summary>
    /// <typeparam name="TModel">The target model to convert to.</typeparam>
    /// <param name="list">The collection of entities to search.</param>
    /// <returns>The first entity whose type matches the specified type name wrapped into the target model, or null if no matching entity is found.</returns>
    public static TModel? OneOfType<TModel>(this IEnumerable<IEntity>? list)
        where TModel : IWrapperSetup<IEntity>, new()
        => list.OneOfType<TModel>(typeof(TModel).Name);


    /// <summary>
    /// Returns the first entity that matches the specified type name, or null if not found.
    /// </summary>
    /// <typeparam name="TModel">The target model to convert to.</typeparam>
    /// <param name="list">The collection of entities to search.</param>
    /// <param name="nameId">The name of the type to match.</param>
    /// <returns>The first entity whose type matches the specified type name wrapped into the target model, or null if no matching entity is found.</returns>
    public static TModel? OneOfType<TModel>(this IEnumerable<IEntity>? list, string nameId)
        where TModel : IWrapperSetup<IEntity>, new()
    {
        if (list == null)
            return default;

        var first = list.OneOfType(nameId);
        return first.As<TModel>();
    }
}
