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
        where TModel : class, IModelFromEntity, new()
        => list.FirstModel<TModel>(options: new());

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
        var stableOptions = (options ?? new()) with
        {
            TypeNameCheck = options?.TypeNameCheck ?? ToModelOptions.ModelTypeCheck.Skip,
        };
        

        if (list == null)
            return ToModelIntern.FromNull<TModel>(trueType: null, stableOptions);

        // Figure out the true type to create, based on Attribute
        // This is important, in case an interface was passed in.
        var trueType = ModelAnalyseUse.GetTargetType<TModel>();

        var nameList = stableOptions.TypeName != null
            ? [stableOptions.TypeName]
            : DataModelAnalyzer.GetValidTypeNames(trueType);

        var firstMatch = nameList.Select(list.First).OfType<IEntity>().FirstOrDefault();
        return firstMatch != null
            ? firstMatch.ToModelInternal<TModel>(options: stableOptions,
                trueType: trueType/*, nullHandling: nullHandling*/)
            // Nothing found
            : ToModelIntern.FromNull<TModel>(trueType, stableOptions/*, nullHandling*/);
    }

    #endregion

}
