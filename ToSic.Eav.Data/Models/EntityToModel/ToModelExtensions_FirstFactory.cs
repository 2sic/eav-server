using ToSic.Eav.Models.Factory;
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
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="typeName">The name of the type to match.</param>
    /// <param name="factory">A factory to create the target model.</param>
    /// <returns>The first entity whose type matches the specified type name wrapped into the target model, or null if no matching entity is found.</returns>
    public static TModel? First<TModel>(
        this IEnumerable<IEntity>? list,
        IModelFactory factory,
        NoParamOrder npo = default,
        string? typeName = default
    )
        where TModel : class, IModelSetup<IEntity>
    {
        if (list == null)
            return default;

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
                return factory.Create<IEntity, TModel>(first);
        }

        // Nothing found
        return default;
    }

    #endregion

}
