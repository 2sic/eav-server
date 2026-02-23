using ToSic.Eav.Data;
using ToSic.Sys.Coding;

namespace ToSic.Eav.Models;

public static class TestAccessors
{
    internal static TModel? AsTac<TModel>(this IEntity? entity)
        where TModel : class, IModelFromEntity, new()
        => entity.ToModel<TModel>();

    internal static TModel? AsTac<TModel>(
        this IEntity? entity,
        NoParamOrder npo = default,
        bool skipTypeCheck = false,
        //bool nullIfNull = false
        ModelNullHandling nullHandling = ModelNullHandling.Undefined
    )
        where TModel : class, IModelFromEntity, new()
        => entity.ToModel<TModel>(npo, skipTypeCheck, nullHandling: nullHandling);

    internal static TModel? AsInternalTac<TModel>(
        this IEntity? entity,
        NoParamOrder npo = default,
        bool skipTypeCheck = false,
        //bool nullIfNull = false
        ModelNullHandling nullHandling = ModelNullHandling.Undefined
    )
        where TModel : class, IModelFromEntity//, new()
        => entity.ToModelInternal<TModel>(npo, skipTypeCheck: skipTypeCheck, nullHandling: nullHandling);

    internal static TModel? FirstTac<TModel>(
        this IEnumerable<IEntity>? list,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        NoParamOrder npo = default,
        string? typeName = default,
        ModelNullHandling nullHandling = ModelNullHandling.Undefined)
        where TModel : class, IModelFromEntity, new()
        => list.FirstModel<TModel>(npo, typeName, nullHandling);

    public static TModel? FirstTac<TModel>(this IEnumerable<IEntity>? list)
        where TModel : class, IModelFromEntity, new()
        => list.FirstModel<TModel>();

}