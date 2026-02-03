using ToSic.Eav.Models;
using ToSic.Eav.Models.Factory;

namespace ToSic.Eav.Data;

public static partial class ModelFactoryExtensions
{
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
    public static IEnumerable<TModel?> GetAll<TModel>(
        this IModelFactory factory,
        IEnumerable<IEntity>? list,
        NoParamOrder npo = default,
        string? typeName = default
    ) where TModel : class, IModelSetup<IEntity>
    {
        if (list == null)
            return [];

        var selection = list
            .GetAll(typeName: typeName ?? typeof(TModel).Name);

        return selection.Select(factory.Create<IEntity, TModel>)
                .ToList();
    }


}
