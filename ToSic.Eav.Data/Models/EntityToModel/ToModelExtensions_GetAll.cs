using ToSic.Eav.Models.Sys;

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
    /// <param name="options">Conversion options for more advanced scenarios</param>
    /// <returns>An enumerable collection of TModel instances wrapping the matching entities. Returns an empty collection if the
    /// source is null or no matching entities are found.</returns>
    public static IEnumerable<TModel> GetModels<TModel>(
        this IEnumerable<IEntity>? list,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        NoParamOrder npo = default,
        ToModelOptions? options = default
    )
        where TModel : class, IModelFromEntity
    {
        // List null - always stop here
        // Not all options listed, as the explicit return-Empty is automatically covered
        if (list == null)
            return [];

        // Figure out the true type to create, based on Attribute
        // This is important, in case an interface was passed in.
        var trueType = ModelFromEntityTypeManagerNoFactory.GetTargetType<TModel>(nameof(GetModels));

        var nameList = ModelContentTypeNameExtractor
            .GetNames(options?.TypeName, typeof(TModel), trueType).Names;

        var firstMatchingList = nameList
            .Select(name => list.GetAll(typeName: name).ToListOpt())
            .FirstOrDefault(found => found.Any());

        if (firstMatchingList == null)
            return [];

        // We'll pre-fetch the exact type to use and do name checks, so any use later on should not do it again
        options = (options ?? new()) with { TypeName = ToModelOptions.TypeNameAny };

        return firstMatchingList
            .Select(raw => raw.ToModelOrNull<TModel>(options: options, trueType: trueType)!)
            .ToList();
        
        // 2026-07-30 2dm - old code, which certainly did short-circuit but not as functional
        // keep for reference for a while, then remove
        //foreach (var name in nameList)
        //{
        //    // ReSharper disable once PossibleMultipleEnumeration - should not do ToList _before_ using this, because it could lose optimizations of the FastLookup etc.
        //    var found = list
        //        .GetAll(typeName: name)
        //        .ToListOpt();

        //    if (!found.Any())
        //        continue;

        //    var result = found
        //        .Select(raw => raw.ToModelInternal<TModel>(options: options, trueType: trueType)!);

        //    return result.ToList();
        //}

        //return [];
    }


}
