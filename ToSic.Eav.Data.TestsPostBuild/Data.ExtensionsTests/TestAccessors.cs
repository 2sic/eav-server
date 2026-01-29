using ToSic.Sys.Coding;
using ToSic.Sys.Wrappers;

namespace ToSic.Eav.Data.ExtensionsTests;

public static class TestAccessors
{
    internal static TModel? AsTac<TModel>(
        this IEntity? entity,
        NoParamOrder npo = default,
        bool skipTypeCheck = false,
        bool nullIfNull = false
    )
        where TModel : class, IWrapperSetup<IEntity>, new()
        => entity.As<TModel>(npo, skipTypeCheck, nullIfNull);


    internal static TModel? FirstTac<TModel>(
        this IEnumerable<IEntity>? list,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        NoParamOrder npo = default,
        string? typeName = default)
        where TModel : class, IWrapperSetup<IEntity>, new()
        => list.First<TModel>(npo, typeName);

    public static TModel? FirstTac<TModel>(this IEnumerable<IEntity>? list)
        where TModel : class, IWrapperSetup<IEntity>, new()
        => list.First<TModel>();

}