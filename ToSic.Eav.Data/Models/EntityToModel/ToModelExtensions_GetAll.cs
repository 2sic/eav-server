using ToSic.Eav.Models.Sys;
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable MethodOverloadWithOptionalParameter

namespace ToSic.Eav.Models;

public static partial class ToModelExtensions
{
    /// <summary>
    /// Filter data by type matching the model, and return as a collection of type `TModel`.
    /// </summary>
    /// <typeparam name="TModel">
    /// The model type to wrap each entity.
    /// Must implement <see cref="IModelFromEntity"/> and should implement <see cref="IModelSetup{IEntity}"/>.
    /// The type-name being filtered is derived from the model name, or from the <see cref="ModelSpecsAttribute"/> on the model.
    /// </typeparam>
    /// <param name="list">The collection of entities to filter and wrap.</param>
    /// <returns>An enumerable collection of wrapped entities of the specified model type. Returns an empty collection if the
    /// input is null or contains no matching entities.</returns>
    public static IEnumerable<TModel> GetModels<TModel>(this IEnumerable<IEntity>? list)
        where TModel : class, IModelFromEntity
        => list.GetModels<TModel>(options: new());

    /// <summary>
    /// Filter data by type matching the model, and return as a collection of type `TModel`.
    /// </summary>
    /// <typeparam name="TModel">
    /// The model type to wrap each entity.
    /// Must implement <see cref="IModelFromEntity"/> and should implement <see cref="IModelSetup{IEntity}"/>.
    /// The type-name being filtered is used from <see cref="typeName"/>, derived from the model name, or from the <see cref="ModelSpecsAttribute"/> on the model.
    /// </typeparam>
    /// <param name="list">The source collection of entities to search. Can be null.</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="options">Conversion options</param>
    /// <returns>An enumerable collection of TModel instances wrapping the matching entities. Returns an empty collection if the
    /// source is null or no matching entities are found.</returns>
    public static IEnumerable<TModel> GetModels<TModel>(this IEnumerable<IEntity>? list, NoParamOrder npo = default, ToModelOptions? options = default)
        where TModel : class, IModelFromEntity
    {
        var specs = ToModelSpecs<TModel>.List(list, options, null, useFactory: false);
        if (specs.ExitEarly)
            return [];
        
        var firstMatchingList = GetModelsDoLookup(specs, list!);

        // We'll pre-fetch the exact type to use and do name checks, so any use later on should not do it again
        specs = specs.DisableNameCheck();

        return firstMatchingList
            ?.Select(raw => raw.ToModelOrNull<TModel>(options: specs.Options, trueType: specs.TrueType)!)
            .ToListOpt()
            ?? [];
    }

    private static IList<IEntity>? GetModelsDoLookup(ToModelSpecs specs, IEnumerable<IEntity> list)
    {
        var nameList = ModelContentTypeNameExtractor.GetNames(specs).Names;

        var firstMatchingList = nameList
            .Select(name => list!.GetAll(typeName: name).ToListOpt())
            .FirstOrDefault(found => found.Any());

        return firstMatchingList;
    }

}
