#if NETFRAMEWORK


// Old stuff for compatibility with DNN, should not bleed into Oqtane
namespace ToSic.Eav.Data;

partial class EntityWrapper
{

    [PrivateApi]
    [Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
    public object GetBestValue(string attributeName, bool resolveHyperlinks)
        => Entity.GetBestValue(attributeName);

    [PrivateApi]
    [Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
    public object GetBestValue(string attributeName, string[] languages, bool resolveHyperlinks)
        => Entity.GetBestValue(attributeName, languages);

    [PrivateApi]
    [Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
    public T GetBestValue<T>(string attributeName, string[] languages, bool resolveHyperlinks)
        => Entity.GetBestValue<T>(attributeName, languages);


    [PrivateApi]
    [Obsolete("Deprecated. Do not use any more, as it cannot reliably know the real language list. Use GetBestValue(name, languageList)")]
    public object GetBestValue(string attributeName) => Entity.GetBestValue(attributeName);

    [PrivateApi]
    [Obsolete("Deprecated. Do not use any more, as it cannot reliably know the real language list. Use GetBestValue(name, languageList)")]
    public TVal GetBestValue<TVal>(string name)
        => Entity.GetBestValue<TVal>(name);


    // 2020-10-30 trying to drop uses with ResolveHyperlinks
    ///// <inheritdoc />
    //public TVal GetBestValue<TVal>(string name, bool resolveHyperlinks = false)
    //    => Entity.GetBestValue<TVal>(name, resolveHyperlinks);

    // 2020-12-15 disabled - I believe it was never in use
    [PrivateApi]
    [Obsolete("was probably never in use anywhere, but we'll leave it alive to not break existing code")]
    public object PrimaryValue(string attributeName) => Entity.Value(attributeName);

    [PrivateApi]
    [Obsolete("was probably never in use anywhere, but we'll leave it alive to not break existing code")]
    public T PrimaryValue<T>(string attributeName) => Entity.Value<T>(attributeName);

    // 2020-12-15 2dm disabled, don't think it was ever in use
    [PrivateApi]
    [Obsolete("was probably never in use anywhere, but we'll leave it alive to not break existing code")]
    public object Value(string field, bool resolve) => Entity.Value(field);

    // 2020-12-15 2dm disabled, don't think it was ever in use
    [PrivateApi]
    [Obsolete("was probably never in use anywhere, but we'll leave it alive to not break existing code")]
    public T Value<T>(string field, bool resolve) => Entity.Value<T>(field);

}

#endif
