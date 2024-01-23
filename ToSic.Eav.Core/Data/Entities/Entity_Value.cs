using ToSic.Eav.Plumbing;
using ToSic.Lib.Coding;

namespace ToSic.Eav.Data;
partial class Entity
{
    /// <inheritdoc />
    public object Value(string field) => GetBestValue(field, null);

    /// <inheritdoc />
    public T Value<T>(string field) => GetBestValue<T>(field, null);


    public object Get(string name) => GetBestValue(name, null);

    public object Get(string name, NoParamOrder noParamOrder = default, string language = default, string[] languages = default) 
        => GetBestValue(name, HandleLanguageParams(language, languages));

    public TValue Get<TValue>(string name) => GetBestValue<TValue>(name, null);

    public TValue Get<TValue>(string name, NoParamOrder noParamOrder = default, TValue fallback = default, string language = default, string[] languages = default) 
        => GetBestValue(name, HandleLanguageParams(language, languages)).ConvertOrFallback(fallback);

    private string[] HandleLanguageParams(string language, string[] languages) 
        => language.SafeAny() ? languages : language.HasValue() ? [language] : null;
}