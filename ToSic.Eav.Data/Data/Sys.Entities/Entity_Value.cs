using ToSic.Eav.Data.Sys.PropertyLookup;

namespace ToSic.Eav.Data.Sys.Entities;
partial record Entity
{
    public object? Get(string name)
        => GetPropertyInternal(new(name), new()).Result;

    //// ReSharper disable once MethodOverloadWithOptionalParameter
    public object? Get(string name, NoParamOrder npo = default, string? language = default, string?[]? languages = default)
        => GetPropertyInternal(new(name, HandleLanguageParams(language, languages), true), new()).Result;

    private static string?[] HandleLanguageParams(string? language, string?[]? languages) 
        => languages.SafeAny()
            ? PropReqSpecs.ExtendDimsWithDefault(languages)
            : language.HasValue()
                ? [language, null]
                : PropReqSpecs.EmptyDimensions;
}