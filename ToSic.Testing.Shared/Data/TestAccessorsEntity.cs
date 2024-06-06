using ToSic.Eav.Data;
using ToSic.Lib.Coding;

namespace ToSic.Testing.Shared.Data;

public static class TestAccessorsEntity
{
    public static object TacGet(this IEntity entity, string name)
        => entity.Get(name);

    public static object TacGet(this IEntity entity, string name, NoParamOrder noParamOrder = default, string language = default, string[] languages = default)
            => entity.Get(name, noParamOrder, language, languages);

    public static TValue TacGet<TValue>(this IEntity entity, string name)
        => entity.Get<TValue>(name);

    public static TValue TacGet<TValue>(this IEntity entity, string name, NoParamOrder noParamOrder = default,
        TValue fallback = default, string language = default, string[] languages = default)
        => entity.Get(name, noParamOrder, fallback, language, languages);

}