/* only in old old .net */

#if NETFRAMEWORK

namespace ToSic.Eav.Data;

partial record Entity
{
    // #RemovedInV20
    //[PrivateApi]
    //[Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
    //public object GetBestValue(string attributeName, string[] languages, bool resolveHyperlinks)
    //{
    //    var set = GetPropertyInternal(new(attributeName, languages, false), null);
    //    return set.Result;
    //}

    // #RemovedInV20
    //[PrivateApi]
    //[Obsolete("Deprecated. Do not use any more, as it cannot reliably know the real language list. Use Get(name, languageList)")]
    //public object GetBestValue(string attributeName) => GetBestValue(attributeName, []);

    // #RemovedInV20
    //[PrivateApi]
    //[Obsolete("Deprecated. Do not use any more, as it cannot reliably resolve hyperlinks.")]
    //public TVal GetBestValue<TVal>(string name) => GetBestValue(name).ConvertOrDefault<TVal>();

    // #RemovedInV20
    //[PrivateApi]
    //[Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
    //public /*new*/ object GetBestValue(string attributeName, bool resolveHyperlinks)
    //    => GetBestValue(attributeName, []);


    // #RemovedInV20
    //[PrivateApi]
    //[Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
    //public TVal GetBestValue<TVal>(string name, string[] languages, bool resolveHyperlinks)
    //    => GetBestValue(name, languages).ConvertOrDefault<TVal>();

    // #RemovedInV20
    //// 2020-12-15 Deprecated this
    //[PrivateApi]
    //[Obsolete("was probably never in use anywhere, but we'll leave it alive to not break existing code")]
    //public object PrimaryValue(string attributeName) => Value(attributeName);

    // #RemovedInV20
    //// 2020-12-15 Deprecated this
    //[PrivateApi]
    //[Obsolete("was probably never in use anywhere, but we'll leave it alive to not break existing code")]
    //public TVal PrimaryValue<TVal>(string attributeName) => Value<TVal>(attributeName);

    // #RemovedInV20
    //// 2020-12-15 2dm disabled, don't think it was ever in use
    //[PrivateApi]
    //[Obsolete("was probably never in use anywhere, but we'll leave it alive to not break existing code")]
    //public object Value(string field, bool resolve) => Value(field);

    // #RemovedInV20
    //[PrivateApi]
    //[Obsolete("was probably never in use anywhere, but we'll leave it alive to not break existing code")]
    //public T Value<T>(string field, bool resolve) => Value(field).ConvertOrDefault<T>();

}

#endif