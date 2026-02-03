using ToSic.Eav.Models;
using ToSic.Eav.Models.Sys;

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
        where TModel : class, IModelSetup<IEntity>, new()
        => list.GetAll<TModel>(typeName: null);

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
    public static IEnumerable<TModel> GetAll<TModel>(
        this IEnumerable<IEntity>? list,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        NoParamOrder npo = default,
        string? typeName = default,
        ModelNullHandling nullHandling = ModelNullHandling.Undefined
    )
        where TModel : class, IModelSetup<IEntity>, new()
    {
        // List null - always stop here
        // Not all options listed, as the explicit return-Empty is automatically covered
        if (list == null)
            return (nullHandling & ModelNullHandling.ListNullThrows) != 0
                ? throw new ArgumentNullException(nameof(list))
                : [];

        var nameList = typeName != null
            ? [typeName]
            : DataModelAnalyzer.GetValidTypeNames<TModel>();

        if (nullHandling == ModelNullHandling.Undefined)
            nullHandling = ModelNullHandling.Default;

        foreach (var name in nameList)
        {
            // ReSharper disable once PossibleMultipleEnumeration - should not do ToList _before_ using this, because it could lose optimizations of the FastLookup etc.
            var found = list
                .GetAll(typeName: name)
                .ToListOpt();

            if (!found.Any())
                continue;

            var result = found
                .Select(raw => raw.AsInternal<TModel>(skipTypeCheck: true, nullHandling: nullHandling)!);
                    
            if ((nullHandling & ModelNullHandling.ModelNullSkip) != 0)
                result = result.Where(item => item != null);

            return result.ToList();
        }

        return [];
    }


}
