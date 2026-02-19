using ToSic.Eav.Models;
using ToSic.Eav.Models.Sys;

namespace ToSic.Eav.Data;

public static partial class EntityListToModelExtensions
{

    #region Generic
    
    /// <summary>
    /// Returns the first entity that matches the specified type name, or null if not found.
    /// </summary>
    /// <typeparam name="TModel">The target model to convert to.</typeparam>
    /// <param name="list">The collection of entities to search.</param>
    /// <returns>The first entity whose type matches the specified type name wrapped into the target model, or null if no matching entity is found.</returns>
    public static TModel? First<TModel>(this IEnumerable<IEntity>? list)
        where TModel : class, IModelSetup<IEntity>, new()
        => list.First<TModel>(typeName: null);

    /// <summary>
    /// Returns the first entity that matches the specified type name, or null if not found.
    /// </summary>
    /// <typeparam name="TModel">The target model to convert to.</typeparam>
    /// <param name="list">The collection of entities to search.</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="typeName">The name of the type to match.</param>
    /// <param name="nullHandling"></param>
    /// <returns>The first entity whose type matches the specified type name wrapped into the target model, or null if no matching entity is found.</returns>
    public static TModel? First<TModel>(
        this IEnumerable<IEntity>? list,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        NoParamOrder npo = default,
        string? typeName = default,
        ModelNullHandling nullHandling = ModelNullHandling.Undefined
    )
        where TModel : class
    {
        if (nullHandling == ModelNullHandling.Undefined)
            nullHandling = ModelNullHandling.Default;

        if (list == null)
            return (nullHandling & ModelNullHandling.ListNullThrows) != 0
                ? throw new ArgumentNullException(nameof(list))
                : AsModelInternal.FromNull<TModel>(trueType: null, nullHandling);

        // Figure out the true type to create, based on Attribute
        // This is important, in case an interface was passed in.
        var trueType = ModelAnalyseUse.GetTargetType<TModel>();

        var nameList = typeName != null
            ? [typeName]
            : DataModelAnalyzer.GetValidTypeNames(trueType);

        foreach (var name in nameList)
        {
            // ReSharper disable once PossibleMultipleEnumeration - should not ToList or anything, because it could lose optimizations of the FastLookup etc.
            var first = list.First(typeName: name);
            if (first != null)
                return first.AsInternal<TModel>(trueType: trueType, skipTypeCheck: true, nullHandling: nullHandling);
        }

        // Nothing found
        return AsModelInternal.FromNull<TModel>(trueType, nullHandling);
    }

    #endregion

}
