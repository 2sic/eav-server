namespace ToSic.Eav.Data;

public static class EntityTestAccessors
{
    public static object? GetTac(this IEntity entity, string name)
        => entity.Get(name);

    public static object? GetTac(this IEntity entity, string name, NoParamOrder npo = default, string language = default, string[] languages = default)
            => entity.Get(name, npo, language, languages);

    public static TValue? GetTac<TValue>(this IEntity entity, string name)
        => entity.Get<TValue>(name);

    public static TValue? GetTac<TValue>(this IEntity entity, string name, NoParamOrder npo = default,
        TValue fallback = default, string language = default, string[] languages = default)
        => entity.Get(name, npo, fallback, language, languages);

}