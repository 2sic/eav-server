﻿/* only in old old .net */

#if NETFRAMEWORK
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Data;

partial record Entity
{

    [PrivateApi]
    [Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
    public object GetBestValue(string attributeName, string[] languages, bool resolveHyperlinks)
    {
        var set = FindPropertyInternal(new(attributeName, languages, false), null);
        return set.Result;
    }

    [PrivateApi]
    [Obsolete("Deprecated. Do not use any more, as it cannot reliably know the real language list. Use GetBestValue(name, languageList)")]
    public object GetBestValue(string attributeName) => GetBestValue(attributeName, []);

    [PrivateApi]
    [Obsolete("Deprecated. Do not use any more, as it cannot reliably resolve hyperlinks.")]
    public TVal GetBestValue<TVal>(string name) => GetBestValue(name).ConvertOrDefault<TVal>();


    [PrivateApi]
    [Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
    public /*new*/ object GetBestValue(string attributeName, bool resolveHyperlinks)
        => GetBestValue(attributeName, []);


    [PrivateApi]
    [Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
    public TVal GetBestValue<TVal>(string name, string[] languages, bool resolveHyperlinks)
        => GetBestValue(name, languages).ConvertOrDefault<TVal>();

    // 2020-12-15 Deprecated this
    [PrivateApi]
    [Obsolete("was probably never in use anywhere, but we'll leave it alive to not break existing code")]
    public object PrimaryValue(string attributeName) => Value(attributeName);

    // 2020-12-15 Deprecated this
    [PrivateApi]
    [Obsolete("was probably never in use anywhere, but we'll leave it alive to not break existing code")]
    public TVal PrimaryValue<TVal>(string attributeName) => Value<TVal>(attributeName);

    // 2020-12-15 2dm disabled, don't think it was ever in use
    [PrivateApi]
    [Obsolete("was probably never in use anywhere, but we'll leave it alive to not break existing code")]
    public object Value(string field, bool resolve) => Value(field);

    [PrivateApi]
    [Obsolete("was probably never in use anywhere, but we'll leave it alive to not break existing code")]
    public T Value<T>(string field, bool resolve) => Value(field).ConvertOrDefault<T>();

}

#endif