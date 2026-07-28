using ToSic.Eav.Data;
using ToSic.Eav.Models.Factory;
using ToSic.Sys.Coding;

namespace ToSic.Eav.Models;

public static class ToModelTestAccessors
{
    #region ToModelInternal

    internal static TModel? ToModelInternalTac<TModel>(
        this IEntity? entity,
        NoParamOrder npo = default,
        bool skipTypeCheck = false,
        ModelNullHandling nullHandling = ModelNullHandling.Undefined
    )
        where TModel : class, IModelFromEntity//, new()
        => entity.ToModelInternal<TModel>(npo, skipTypeCheck: skipTypeCheck, nullHandling: nullHandling);

    #endregion

    #region ToModel (Single Entity / CanBeEntity)

    internal static TModel? ToModelTac<TModel>(this IEntity? entity)
        where TModel : class, IModelFromEntity, new()
        => entity.ToModel<TModel>();

    internal static TModel? ToModelTac<TModel>(
        this IEntity? entity,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        NoParamOrder npo = default,
        bool skipTypeCheck = false,
        ModelNullHandling nullHandling = ModelNullHandling.Undefined
    )
        where TModel : class, IModelFromEntity, new()
        => entity.ToModel<TModel>(npo, skipTypeCheck, nullHandling: nullHandling);

    

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
        string? typeName = default,
        ModelNullHandling nullHandling = ModelNullHandling.Undefined)
        where TModel : class, IModelFromEntity, new()
        => list.FirstModel<TModel>(npo, typeName, nullHandling);

    public static TModel? FirstModelTac<TModel>(this IEnumerable<IEntity>? list)
        where TModel : class, IModelFromEntity, new()
        => list.FirstModel<TModel>();

}