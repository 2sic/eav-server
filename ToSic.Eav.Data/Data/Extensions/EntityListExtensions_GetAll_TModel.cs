namespace ToSic.Eav.Data;

public static partial class EntityListExtensions
{
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
        where TModel : class, IWrapperSetup<IEntity>, new()
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
        where TModel : class, IWrapperSetup<IEntity>, new()
    {
        if (list == null)
            return [];

        var result = list
            .GetAll(typeName: typeName)
            .Select(raw => raw.AsInternal<TModel>(skipTypeCheck: true)!)
            .ToList();

        return result;
    }


}
