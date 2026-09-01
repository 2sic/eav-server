using ToSic.Eav.Data;
using ToSic.Eav.Models.Factory;

namespace ToSic.Eav.Models;

public static class ToModelTestAccessors
{
    #region ToModelInternal

    internal static TModel? ToModelOrNullTac<TModel>(this IEntity? entity, ToModelOptions options, NoParamOrder npo = default)
        where TModel : class, IModelFromEntity
        => ToModelExtensions.ToModelInternal<TModel>(entity, options);


    #endregion

    
    #region ToModel (Single Entity / CanBeEntity)

    internal static TModel? ToModelTac<TModel>(this IEntity? entity)
        where TModel : class, IModelFromEntity
        => entity.ToModel<TModel>();

    // ReSharper disable once MethodOverloadWithOptionalParameter
    internal static TModel? ToModelTac<TModel>(this IEntity? entity, NoParamOrder npo = default, ToModelOptions? options = default)
        where TModel : class, IModelFromEntity
        => entity.ToModel<TModel>(options: options);


    public static TModel? ToModelTac<TModel>(this IEntity entity, IModelFactory factory, NoParamOrder npo = default, ToModelOptions? options = default)
        where TModel : class, IModelFromEntity
        => entity.ToModel<TModel>(factory, npo, options);
    

    /// <summary>
    /// ICanBeEntity Overload
    /// </summary>
    public static TModel? ToModelTac<TModel>(this ICanBeEntity? canBeEntity, NoParamOrder npo = default, ToModelOptions? options = default)
        where TModel : class, IModelFromEntity
    => canBeEntity.ToModel<TModel>(npo, options: options);

    #endregion


    #region ToModel with Factory (Single Entity / CanBeEntity TODO:)

    public static TModel? ToModelTac<TModel>(this IEntity entity, IModelFactory factory)
        where TModel : class, IModelFromEntity
        => entity.ToModel<TModel>(factory);


    #endregion

    
    #region First

    public static TModel? FirstModelTac<TModel>(this IEnumerable<IEntity>? list)
        where TModel : class, IModelFromEntity, new()
        => list.FirstModel<TModel>();

    public static TModel? FirstModelTac<TModel>(
        this IEnumerable<IEntity>? list,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        NoParamOrder npo = default,
        ToModelOptions? options = default
    )
        where TModel : class, IModelFromEntity, new()
        => list.FirstModel<TModel>(npo, options);

    #endregion

}