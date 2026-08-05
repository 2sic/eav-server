using ToSic.Eav.Models.Factory;
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
    public static IEnumerable<TModel?> GetModels<TModel>(this IEnumerable<IEntity>? list)
        where TModel : class, IModelFromEntity
        => GetModelsInternal<TModel>(list, options: new(), factory: null);

    
    
    /// <summary>
    /// Filter data by type matching the model, and return as a collection of type `TModel`.
    /// </summary>
    /// <typeparam name="TModel">
    /// The model type to wrap each entity.
    /// Must implement <see cref="IModelFromEntity"/> and should implement <see cref="IModelSetup{IEntity}"/>.
    /// The type-name being filtered is used from <see cref="ToModelOptions"/>, derived from the model name, or from the <see cref="ModelSpecsAttribute"/> on the model.
    /// </typeparam>
    /// <param name="list">The source collection of entities to search. Can be null.</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="options">Conversion options</param>
    /// <returns>An enumerable collection of TModel instances wrapping the matching entities. Returns an empty collection if the
    /// source is null or no matching entities are found.</returns>
    public static IEnumerable<TModel?> GetModels<TModel>(this IEnumerable<IEntity>? list, NoParamOrder npo = default, ToModelOptions? options = default)
        where TModel : class, IModelFromEntity
        => GetModelsInternal<TModel>(list, options: options, factory: null);

    
    
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
        => GetModelsInternal<TModel>(list: list, options: options, factory: AssertFactory(factory));


    
    private static IEnumerable<TModel?> GetModelsInternal<TModel>(IEnumerable<IEntity>? list, ToModelOptions? options, IModelFactory? factory)
        where TModel : class, IModelFromEntity
    {
        var specs = ToModelSpecs<TModel>.List(list, options, null, factory);
        if (specs.ExitEarly)
            return [];

        var nameList = ModelContentTypeNameExtractor.GetNames(specs).Names;

        var firstMatchingList = nameList
            .Select(name => list!.GetAll(typeName: name).ToListOpt())
            .FirstOrDefault(found => found.Any())
            ?? [];

        var optionsSkipNameCheck = specs.OptionsDisableNameCheck();
        
        return factory != null
            ? firstMatchingList
                .Select(item => factory.Create<IEntity, TModel>(item, optionsSkipNameCheck))
                .ToListOpt()
            : firstMatchingList
                .Select(raw => ToModelInternal<TModel>(raw, options: optionsSkipNameCheck, trueType: specs.TrueType)!)
                .ToListOpt();
    }
}
