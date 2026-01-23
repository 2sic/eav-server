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
    [ShowApiWhenReleased(ShowApiMode.Never)]
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

    /// <summary>
    /// Returns a collection of wrapper objects of type `TModel` for all entities of the specified type.
    /// </summary>
    /// <typeparam name="TModel">
    /// The model type to wrap each entity.
    /// Must implement `IWrapperSetup{IEntity}` and have a parameterless constructor.
    /// </typeparam>
    /// <param name="list">The collection of entities to filter and wrap.</param>
    /// <returns>An enumerable collection of wrapped entities of the specified model type. Returns an empty collection if the
    /// input is null or contains no matching entities.</returns>
    public static IEnumerable<TModel> GetAll<TModel>(this IEnumerable<IEntity>? list)
        where TModel : IWrapperSetup<IEntity>, new()
        => list.GetAll<TModel>(typeName: typeof(TModel).Name);

    /// <summary>
    /// Returns a collection of wrapper objects of type `TModel` for all entities of the specified type name.
    /// </summary>
    /// <typeparam name="TModel">
    /// The model type to wrap each entity.
    /// Must implement `IWrapperSetup{IEntity}` and have a parameterless constructor.
    /// </typeparam>
    /// <param name="list">The source collection of entities to search. Can be null.</param>
    /// <param name="typeName">The name identifier of the entity type to filter by. This value is used to select entities of a specific type.</param>
    /// <returns>An enumerable collection of TModel instances wrapping the matching entities. Returns an empty collection if the
    /// source is null or no matching entities are found.</returns>
    public static IEnumerable<TModel> GetAll<TModel>(this IEnumerable<IEntity>? list, string typeName)
        where TModel : IWrapperSetup<IEntity>, new()
    {
        if (list == null)
            return [];

        var result = list
            .GetAll(typeName: typeName)
            .Select(raw => raw.As<TModel>()!)
            .ToList();

        return result;
    }

    /// <summary>
    /// Returns a collection of wrapper objects of type `TModel` for all entities of the specified type name.
    /// </summary>
    /// <typeparam name="TModel">
    /// The model type to wrap each entity.
    /// Must implement `IWrapperSetup{IEntity}` and have a parameterless constructor.
    /// </typeparam>
    /// <param name="list">The source collection of entities to search. Can be null.</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="typeName">The name identifier of the entity type to filter by. This value is used to select entities of a specific type.</param>
    /// <returns>An enumerable collection of TModel instances wrapping the matching entities. Returns an empty collection if the
    /// source is null or no matching entities are found.</returns>
    /// <param name="factory">The factory to use for creating wrapper instances.</param>
    // ReSharper disable once MethodOverloadWithOptionalParameter
    public static IEnumerable<TModel> GetAll<TModel>(this IEnumerable<IEntity>? list, NoParamOrder npo = default, string? typeName = default, IWrapperFactory? factory = null)
        where TModel : IWrapperSetup<IEntity>, new()
    {
        if (list == null)
            return [];

        var selection = list
            .GetAll(typeName: typeName ?? typeof(TModel).Name);

        return factory == null
            ? selection.Select(entity => entity.As<TModel>()!).ToList()
            : selection.Select(factory.Create<IEntity, TModel>)
                .ToList();
    }


}
