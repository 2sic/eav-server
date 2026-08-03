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
        => list.FirstModel<TModel>(options: default);

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
    {
        var specs = ToModelSpecs<TModel>.List(list, options, null, useFactory: false);
        if (specs.ExitEarly)
            return specs.Result;

        // For further processing, make sure that it won't re-check the type name unless explicitly specified
        var firstMatch = FirstDoLookup(specs, list!);
        
        return firstMatch.ToModelOrNull<TModel>(options: specs.DisableNameCheck().Options, trueType: specs.TrueType);
    }


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
    {
        var specs = ToModelSpecs<TModel>.List(list, options, null, useFactory: true);
        if (specs.ExitEarly)
            return specs.Result;

        var firstMatch = FirstDoLookup(specs, list!);

        // Nothing found
        return firstMatch != null
            ? factory.Create<IEntity, TModel>(firstMatch)
            : default;
    }



    private static IEntity? FirstDoLookup(ToModelSpecs specs, IEnumerable<IEntity> list)
    {
        var nameList = ModelContentTypeNameExtractor.GetNames(specs).Names;

        var firstMatch = nameList
            .Select(list!.First)
            .OfType<IEntity>()
            .FirstOrDefault();
        return firstMatch;
    }


}
