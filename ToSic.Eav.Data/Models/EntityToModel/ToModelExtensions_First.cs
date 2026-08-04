using ToSic.Eav.Models.Factory;
using ToSic.Eav.Models.Sys;
// ReSharper disable MethodOverloadWithOptionalParameter
// ReSharper disable PossibleMultipleEnumeration

namespace ToSic.Eav.Models;

public static partial class ToModelExtensions
{
    /// <summary>
    /// Returns the first entity that matches the specified type name, or null if not found.
    /// </summary>
    /// <typeparam name="TModel">The target model to convert to.</typeparam>
    /// <param name="list">The collection of entities to search.</param>
    /// <returns>The first entity whose type matches the specified type name wrapped into the target model, or null if no matching entity is found.</returns>
    public static TModel? FirstModel<TModel>(this IEnumerable<IEntity>? list)
        where TModel : class, IModelFromEntity
        => FirstModelInternal<TModel>(list, options: default, factory: null);


    
    /// <summary>
    /// Returns the first entity that matches the specified type name, or null if not found.
    /// </summary>
    /// <typeparam name="TModel">The target model to convert to.</typeparam>
    /// <param name="list">The collection of entities to search.</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="options">Conversion options</param>
    /// <returns>The first entity whose type matches the specified type name wrapped into the target model, or null if no matching entity is found.</returns>
    public static TModel? FirstModel<TModel>(this IEnumerable<IEntity>? list, NoParamOrder npo = default, ToModelOptions? options = default)
        where TModel : class, IModelFromEntity
        => FirstModelInternal<TModel>(list, options: options, factory: null);


    
    /// <summary>
    /// Returns the first entity that matches the specified type name, or null if not found.
    /// </summary>
    /// <typeparam name="TModel">The target model to convert to.</typeparam>
    /// <param name="list">The collection of entities to search.</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="factory">A factory to create the target model.</param>
    /// <param name="options">Conversion options</param>
    /// <returns>The first entity whose type matches the specified type name wrapped into the target model, or null if no matching entity is found.</returns>
    public static TModel? FirstModel<TModel>(this IEnumerable<IEntity>? list, IModelFactory factory, NoParamOrder npo = default, ToModelOptions? options = null)
        where TModel : class, IModelFromEntity
        => FirstModelInternal<TModel>(list, options: options, factory: AssertFactory(factory));


    
    /// <summary>
    /// Main work horse for FirstModel, used by both overloads.
    /// It handles the common logic of filtering and selecting the first entity, and then delegates the final conversion to the provided function.
    /// </summary>
    private static TModel? FirstModelInternal<TModel>(IEnumerable<IEntity>? list, ToModelOptions? options, IModelFactory? factory)
        where TModel : class, IModelFromEntity
    {
        var specs = ToModelSpecs<TModel>.List(list, options, null, factory);
        if (specs.ExitEarly)
            return specs.CreateFromNull();

        var nameList = ModelContentTypeNameExtractor.GetNames(specs).Names;

        var firstMatch = nameList
            .Select(list!.First)
            .OfType<IEntity>()
            .FirstOrDefault();

        // Process result
        var optionsSkipNameCheck = specs.OptionsDisableNameCheck();
        return factory != null
            ? factory.Create<IEntity, TModel>(firstMatch, optionsSkipNameCheck)
            : ToModelInternal<TModel>(firstMatch, options: optionsSkipNameCheck, trueType: specs.TrueType);
    }

}
