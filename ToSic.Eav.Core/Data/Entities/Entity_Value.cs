using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Internal.Generics;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Coding;

namespace ToSic.Eav.Data;
partial record Entity
{
    /// <inheritdoc />
    public object Value(string field)
        // till 17.10 GetBestValue(field, null);
        => FindPropertyInternal(new(field), null).Result;

    /// <inheritdoc />
    public TValue Value<TValue>(string field)
        // till 17.10 GetBestValue<T>(field, null);
        => FindPropertyInternal(new(field), null).Result
            .ConvertOrDefault<TValue>();

    public object Get(string name)
        // till v17.10 GetBestValue(name, null);
        => FindPropertyInternal(new(name), null).Result;

    // ReSharper disable once MethodOverloadWithOptionalParameter
    public object Get(string name, NoParamOrder noParamOrder = default, string language = default, string[] languages = default) 
        // till v17.10 GetBestValue(name, HandleLanguageParams(language, languages));
        => FindPropertyInternal(new(name, HandleLanguageParams(language, languages), true), null).Result;

    public TValue Get<TValue>(string name)
        // till v17.10 GetBestValue<TValue>(name, null);
        => FindPropertyInternal(new(name), null).Result
            .ConvertOrDefault<TValue>();

    // ReSharper disable once MethodOverloadWithOptionalParameter
    public TValue Get<TValue>(string name, NoParamOrder noParamOrder = default, TValue fallback = default, string language = default, string[] languages = default)
        // till v17.10 GetBestValue(name, HandleLanguageParams(language, languages)).ConvertOrFallback(fallback);
        => FindPropertyInternal(new(name, HandleLanguageParams(language, languages), true), null).Result
            .ConvertOrFallback(fallback);

    private static string[] HandleLanguageParams(string language, string[] languages) 
        => languages.SafeAny()
            ? PropReqSpecs.ExtendDimsWithDefault(languages)
            : language.HasValue()
                ? [language, null]
                : PropReqSpecs.EmptyDimensions;
}