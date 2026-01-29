using ToSic.Eav.Models;

namespace ToSic.Eav.Data;

public static partial class EntityListExtensions
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
        bool skipTypeCheck = false
    ) where TModel : class, IModelSetup<IEntity>, new() =>
        // Note: if null / nothing found, let the model decide if it should wrap or return null
        (list?.GetOne(id)).AsInternal<TModel>(skipTypeCheck: skipTypeCheck);

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
        bool nullIfNull = false
    ) where TModel : class, IModelSetup<IEntity>, new() =>
        // Note: if null / nothing found, let the model decide if it should wrap or return null
        (list?.GetOne(guid)).AsInternal<TModel>(skipTypeCheck: skipTypeCheck, nullIfNull: nullIfNull);

}
