namespace ToSic.Eav.Models;

public static partial class ToModelExtensions
{
    /// <summary>
    /// WIP
    /// Convert an IEntity to a model of type TModel.
    /// This is only meant for simple models that do not require a factory.
    /// </summary>
    /// <typeparam name="TModel">TModel must implement IWrapperSetup&lt;IEntity&gt; and have a parameterless constructor.</typeparam>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    public static TModel? ToModel<TModel>(this IEntity? entity)
        where TModel : class, IModelOfEntity
    {
        return entity.ToModelInternal<TModel>();
    }

    /// <summary>
    /// WIP
    /// Convert an IEntity to a model of type TModel.
    /// This is only meant for simple models that do not require a factory.
    /// </summary>
    /// <typeparam name="TModel">TModel must implement IWrapperSetup&lt;IEntity&gt; and have a parameterless constructor.</typeparam>
    /// <param name="entity"></param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="skipTypeCheck">allow conversion even if the Content-Type of the entity doesn't match the type specified in the parameter T</param>
    /// <param name="nullHandling">How to handle nulls during the conversion - default is <see cref="ModelNullHandling.Default"/></param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    public static TModel? ToModel<TModel>(
        this IEntity? entity,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        NoParamOrder npo = default,
        bool skipTypeCheck = false,
        ModelNullHandling nullHandling = ModelNullHandling.Undefined
    )
        where TModel : class, IModelOfEntity
    {
        return entity.ToModelInternal<TModel>(skipTypeCheck: skipTypeCheck, nullHandling: nullHandling);
    }

    /// <summary>
    /// WIP
    /// Convert something which can be an entity to a model of type TModel.
    /// This is only meant for simple models that do not require a factory.
    /// </summary>
    /// <typeparam name="TModel">TModel must implement IWrapperSetup&lt;IEntity&gt; and have a parameterless constructor.</typeparam>
    /// <param name="canBeEntity"></param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="skipTypeCheck">allow conversion even if the Content-Type of the entity doesn't match the type specified in the parameter T</param>
    /// <param name="nullHandling">How to handle nulls during the conversion - default is <see cref="ModelNullHandling.Default"/></param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    public static TModel? ToModel<TModel>(
        this ICanBeEntity? canBeEntity,
        NoParamOrder npo = default,
        bool skipTypeCheck = false,
        ModelNullHandling nullHandling = ModelNullHandling.Undefined
    )
        where TModel : class, IModelOfEntity
    {
        return (canBeEntity?.Entity).ToModelInternal<TModel>(skipTypeCheck: skipTypeCheck);
    }


}