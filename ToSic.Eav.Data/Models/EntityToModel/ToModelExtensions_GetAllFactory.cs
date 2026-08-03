using ToSic.Eav.Models.Factory;
using ToSic.Eav.Models.Sys;
// ReSharper disable PossibleMultipleEnumeration

namespace ToSic.Eav.Models;

public static partial class ToModelExtensions
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
    /// <param name="options">Conversion options</param>
    /// <returns>An enumerable collection of TModel instances wrapping the matching entities. Returns an empty collection if the
    /// source is null or no matching entities are found.</returns>
    /// <param name="factory">The factory to use for creating wrapper instances.</param>
    // ReSharper disable once MethodOverloadWithOptionalParameter
    public static IEnumerable<TModel?> GetModels<TModel>(this IEnumerable<IEntity>? list, IModelFactory factory, NoParamOrder npo = default, ToModelOptions? options = default)
        where TModel : class, IModelFromEntity
    {
        var specs = ToModelSpecs<TModel>.List(list, options, null, useFactory: true);
        if (specs.ExitEarly)
            return [];

        var firstMatchingList = GetModelsDoLookup(specs, list!);

        return firstMatchingList
            ?.Select(factory.Create<IEntity, TModel>)
            .ToListOpt()
            ?? [];
    }
}
