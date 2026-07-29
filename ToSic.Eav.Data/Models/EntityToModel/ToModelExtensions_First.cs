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
    /// <param name="nullHandling"></param>
    /// <returns>The first entity whose type matches the specified type name wrapped into the target model, or null if no matching entity is found.</returns>
    public static TModel? FirstModel<TModel>(
        this IEnumerable<IEntity>? list,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        NoParamOrder npo = default,
        ToModelOptions? options = default,
        ModelNullHandling nullHandling = ModelNullHandling.Undefined
    )
        where TModel : class, IModelFromEntity
    {
        if (nullHandling == ModelNullHandling.Undefined)
            nullHandling = ModelNullHandling.Default;

        if (list == null)
            return
                //(nullHandling & ModelNullHandling.ListNullThrows) != 0
                //? throw new ArgumentNullException(nameof(list))
                //: 
                ToModelIntern.FromNull<TModel>(trueType: null, nullHandling);

        // Figure out the true type to create, based on Attribute
        // This is important, in case an interface was passed in.
        var trueType = ModelAnalyseUse.GetTargetType<TModel>();

        var nameList = options?.TypeName != null
            ? [options.TypeName]
            : DataModelAnalyzer.GetValidTypeNames(trueType);

        var firstMatch = nameList.Select(list.First).OfType<IEntity>().FirstOrDefault();
        return firstMatch != null
            ? firstMatch.ToModelInternal<TModel>(options: new()
                {
                    TypeNameCheck = ToModelOptions.ModelTypeCheck.Skip
                },
                trueType: trueType, nullHandling: nullHandling)
            // Nothing found
            : ToModelIntern.FromNull<TModel>(trueType, nullHandling);
    }

    #endregion

}
