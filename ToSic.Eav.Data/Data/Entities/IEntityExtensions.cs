namespace ToSic.Eav.Data;

/// <summary>
/// Extensions for IEntity to provide additional functionality or utilities.
/// </summary>
/// <remarks>
/// Introduced in v20, moving methods which were previously in `IEntity` to here, since they are better as extensions.
/// </remarks>
[PublicApi]
public static class IEntityExtensions
{
    /// <summary>
    /// Get a value in the expected type from this entity.
    /// </summary>
    /// <typeparam name="TValue">The type to try-convert the result to</typeparam>
    /// <param name="entity"></param>
    /// <param name="name">the field/attribute name</param>
    /// <returns></returns>
    /// <remarks>
    /// If you want to supply a `fallback` it will automatically use the other version of this method
    /// 
    /// History
    /// 
    /// * Introduced as beta in 15.06, published in v17
    /// * Moved from IEntity to here in v20, as it is more of an extension than a core method of IEntity
    /// </remarks>
    public static TValue? Get<TValue>(this IEntity entity, string name)
        => entity.Get(name).ConvertOrDefault<TValue>();

    /// <summary>
    /// Get a value in the expected type from this entity - or a fallback value instead.
    /// </summary>
    /// <typeparam name="TValue">The type to try-convert the result to</typeparam>
    /// <param name="entity"></param>
    /// <param name="name">the field/attribute name</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="fallback">value to be returned if finding or conversion it didn't succeed</param>
    /// <param name="language">optional language like `en-us`</param>
    /// <param name="languages">optional list of language IDs which can be a list which is checked in the order provided</param>
    /// <returns></returns>
    /// <remarks>
    /// History
    /// 
    /// * Introduced as beta in 15.06, published in v17
    /// * Moved from IEntity to here in v20, as it is more of an extension than a core method of IEntity
    /// </remarks>
    // ReSharper disable once MethodOverloadWithOptionalParameter
    public static TValue? Get<TValue>(this IEntity entity, string name, NoParamOrder npo = default, TValue? fallback = default, string? language = default, string?[]? languages = default)
        => entity.Get(name: name, language: language, languages: languages)
            .ConvertOrFallback(fallback);

}
