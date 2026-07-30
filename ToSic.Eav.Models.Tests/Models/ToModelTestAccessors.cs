using ToSic.Eav.Data;
using ToSic.Eav.Models.Factory;
using ToSic.Sys.Coding;

namespace ToSic.Eav.Models;

public static class ToModelTestAccessors
{
    #region ToModelInternal

    internal static TModel? ToModelInternalTac<TModel>(
        this IEntity? entity,
        ToModelOptions options,
        NoParamOrder npo = default
    )
        where TModel : class, IModelFromEntity => entity.ToModelInternal<TModel>(options, npo);

    #endregion

    #region ToModel (Single Entity / CanBeEntity)

    internal static TModel? ToModelTac<TModel>(this IEntity? entity)
        where TModel : class, IModelFromEntity
        => entity.ToModel<TModel>();

    internal static TModel? ToModelTac<TModel>(
        this IEntity? entity,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        NoParamOrder npo = default,
        ToModelOptions? options = default
    )
        where TModel : class, IModelFromEntity
        => entity.ToModel<TModel>(npo, options: options);

    /// <summary>
    /// ICanBeEntity Overload
    /// </summary>
    public static TModel? ToModelTac<TModel>(
        this ICanBeEntity? canBeEntity,
        NoParamOrder npo = default,
        ToModelOptions? options = default
    )
        where TModel : class, IModelFromEntity
    => canBeEntity.ToModel<TModel>(npo, options: options);

    #endregion


    #region ToModel with Factory (Single Entity / CanBeEntity TODO:)

    public static TModel? ToModelTac<TModel>(this IEntity entity, IModelFactory factory)
        where TModel : class, IModelFromEntity
        => entity.ToModel<TModel>(factory);


    #endregion


    internal static TModel? FirstModelTac<TModel>(
        this IEnumerable<IEntity>? list,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        NoParamOrder npo = default,
        ToModelOptions? options = default
    )
        where TModel : class, IModelFromEntity, new()
        => list.FirstModel<TModel>(npo, options);

    public static TModel? FirstModelTac<TModel>(this IEnumerable<IEntity>? list)
        where TModel : class, IModelFromEntity, new()
        => list.FirstModel<TModel>();

}