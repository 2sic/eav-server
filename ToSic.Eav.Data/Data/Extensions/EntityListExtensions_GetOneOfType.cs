using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Model.Sys;

namespace ToSic.Eav.Data;

public static partial class EntityListExtensions
{
    /// <summary>
    /// Returns the first entity that matches the specified type name, or null if not found.
    /// </summary>
    /// <param name="list">The collection of entities to search.</param>
    /// <param name="typeName">The name of the type to match. This comparison is case-sensitive.</param>
    /// <returns>The first entity whose type matches the specified type name, or null if no matching entity is found.</returns>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IEntity? GetOne(this IEnumerable<IEntity> list, string typeName)
    {
        return SysPerfSettings.CacheListAutoIndex && list is ImmutableSmartList fastList
            ? fastList.Fast.OfType(typeName).FirstOrDefault()
            : list.FirstOrDefault(e => e.Type.Is(typeName));
    }

    /// <summary>
    /// Returns the first entity that matches the specified type name, or null if not found.
    /// </summary>
    /// <typeparam name="TModel">The target model to convert to.</typeparam>
    /// <param name="list">The collection of entities to search.</param>
    /// <returns>The first entity whose type matches the specified type name wrapped into the target model, or null if no matching entity is found.</returns>
    public static TModel? GetOne<TModel>(this IEnumerable<IEntity>? list)
        where TModel : class, IWrapperSetup<IEntity>, new()
        => list.GetOne<TModel>(typeName: typeof(TModel).Name);


    ///// <summary>
    ///// Returns the first entity that matches the specified type name, or null if not found.
    ///// </summary>
    ///// <typeparam name="TModel">The target model to convert to.</typeparam>
    ///// <param name="list">The collection of entities to search.</param>
    ///// <param name="typeName">The name of the type to match.</param>
    ///// <returns>The first entity whose type matches the specified type name wrapped into the target model, or null if no matching entity is found.</returns>
    //public static TModel? GetOne<TModel>(this IEnumerable<IEntity>? list, string? typeName)
    //    where TModel : class, IWrapperSetup<IEntity>, new()
    //{
    //    if (list == null)
    //        return default;

    //    var nameList = typeName != null
    //        ? [typeName]
    //        : DataModelAnalyzer.GetContentTypeNamesList<TModel>();

    //    foreach (var name in nameList)
    //    {
    //        var first = list.GetOne(typeName: name);
    //        if (first != null)
    //            return first.As<TModel>();
    //    }

    //    // Must explicitly return null if first not found
    //    // because the As will often return an empty wrapper,
    //    // which is not expected in this case
    //    return default;
    //}

    /// <summary>
    /// Returns the first entity that matches the specified type name, or null if not found.
    /// </summary>
    /// <typeparam name="TModel">The target model to convert to.</typeparam>
    /// <param name="list">The collection of entities to search.</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="typeName">The name of the type to match.</param>
    /// <param name="factory">A factory to create the target model.</param>
    /// <returns>The first entity whose type matches the specified type name wrapped into the target model, or null if no matching entity is found.</returns>
    // ReSharper disable once MethodOverloadWithOptionalParameter
    public static TModel? GetOne<TModel>(this IEnumerable<IEntity>? list, NoParamOrder npo = default, string? typeName = default, IWrapperFactory? factory = null)
        where TModel : class, IWrapperSetup<IEntity>, new()
    {
        if (list == null)
            return default;

        var nameList = typeName != null
            ? [typeName]
            : DataModelAnalyzer.GetContentTypeNamesList<TModel>();

        foreach (var name in nameList)
        {
            var first = list.GetOne(typeName: name);
            if (first != null)
                return factory == null
                    ? first.As<TModel>()
                    : factory.Create<IEntity, TModel>(first);
        }

        // Nothing found
        return default;
    }
}
