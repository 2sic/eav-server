namespace ToSic.Eav.Models;

public static partial class ToModelsExtensions
{

    /// <summary>
    /// Returns the first entity that matches the specified type name, or null if not found.
    /// </summary>
    /// <typeparam name="TModel">The target model to convert to.</typeparam>
    /// <param name="list">The collection of entities to search.</param>
    /// <param name="id"></param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="skipTypeCheck"></param>
    /// <returns>The first entity whose type matches the specified type name wrapped into the target model, or null if no matching entity is found.</returns>
    public static TModel? GetOne<TModel>(
        this IEnumerable<IEntity>? list,
        int id,
        NoParamOrder npo = default,
        bool skipTypeCheck = false,
        ModelNullHandling nullHandling = ModelNullHandling.Undefined
    ) where TModel : class, IModelSetup<IEntity>, new() =>
        // Note: if null / nothing found, let the model decide if it should wrap or return null
        (list?.GetOne(id)).ToModelInternal<TModel>(skipTypeCheck: skipTypeCheck, nullHandling: nullHandling);

    /// <summary>
    /// Returns the first entity that matches the specified type name, or null if not found.
    /// </summary>
    /// <typeparam name="TModel">The target model to convert to.</typeparam>
    /// <param name="list">The collection of entities to search.</param>
    /// <param name="guid"></param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="skipTypeCheck"></param>
    /// <returns>The first entity whose type matches the specified type name wrapped into the target model, or null if no matching entity is found.</returns>
    public static TModel? GetOne<TModel>(
        this IEnumerable<IEntity>? list,
        Guid guid,
        NoParamOrder npo = default,
        bool skipTypeCheck = false,
        //bool nullIfNull = false,
        ModelNullHandling nullHandling = ModelNullHandling.Undefined
    ) where TModel : class, IModelSetup<IEntity>, new() =>
        // Note: if null / nothing found, let the model decide if it should wrap or return null
        (list?.GetOne(guid)).ToModelInternal<TModel>(skipTypeCheck: skipTypeCheck, /*nullIfNull: nullIfNull,*/ nullHandling: nullHandling);

}
