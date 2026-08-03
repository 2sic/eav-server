using ToSic.Eav.Models.Sys;

namespace ToSic.Eav.Models;

public static partial class ToModelExtensions
{

    #region Generic

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
    /// <param name="options">Conversion options for more advanced scenarios</param>
    /// <returns>The first entity whose type matches the specified type name wrapped into the target model, or null if no matching entity is found.</returns>
    public static TModel? FirstModel<TModel>(
        this IEnumerable<IEntity>? list,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        NoParamOrder npo = default,
        ToModelOptions? options = default
    )
        where TModel : class, IModelFromEntity
    {
        var specs = ToModelSpecs<TModel>.List(list, options, null, useFactory: false, methodName: nameof(FirstModel));
        if (specs.ExitEarly)
            return specs.Result;

        (_, _, var trueType, options) = specs;

        var nameList = ModelContentTypeNameExtractor
            .GetNames(options.TypeName, typeof(TModel), trueType).Names;

        var firstMatch = nameList
            .Select(list!.First)
            .OfType<IEntity>()
            .FirstOrDefault();
        
        // For further processing, make sure that it won't re-check the type name unless explicitly specified
        options = options with
        {
            TypeName = options.TypeName ?? ToModelOptions.TypeNameAny,
        };

        return firstMatch.ToModelOrNull<TModel>(options: options, trueType: trueType);
    }

    #endregion

}
