﻿using ToSic.Eav.Data.Sys;
using ToSic.Lib.Coding;

namespace ToSic.Eav.Data.Entities.Sys;
partial record Entity
{
    // #DropOldIEntityValue
    ///// <inheritdoc />
    //public object? Value(string field)
    //    => GetPropertyInternal(new(field), null).Result;

    // #DropOldIEntityValue
    ///// <inheritdoc />
    //public TValue? Value<TValue>(string field)
    //    => GetPropertyInternal(new(field), null).Result
    //        .ConvertOrDefault<TValue>();

    public object? Get(string name)
        => GetPropertyInternal(new(name), null).Result;

    //// ReSharper disable once MethodOverloadWithOptionalParameter
    public object? Get(string name, NoParamOrder noParamOrder = default, string? language = default, string?[]? languages = default)
        => GetPropertyInternal(new(name, HandleLanguageParams(language, languages), true), null).Result;

    // 2025-06-13 #MoveIEntityTypedGetToExtension
    //public TValue? Get<TValue>(string name)
    //    => GetPropertyInternal(new(name), null).Result
    //        .ConvertOrDefault<TValue>();


    // 2025-06-13 #MoveIEntityTypedGetToExtension
    //// ReSharper disable once MethodOverloadWithOptionalParameter
    //public TValue? Get<TValue>(string name, NoParamOrder noParamOrder = default, TValue? fallback = default, string? language = default, string[]? languages = default)
    //    => GetPropertyInternal(new(name, HandleLanguageParams(language, languages), true), null).Result
    //        .ConvertOrFallback(fallback);

    private static string?[] HandleLanguageParams(string? language, string?[]? languages) 
        => languages.SafeAny()
            ? PropReqSpecs.ExtendDimsWithDefault(languages)
            : language.HasValue()
                ? [language, null]
                : PropReqSpecs.EmptyDimensions;
}