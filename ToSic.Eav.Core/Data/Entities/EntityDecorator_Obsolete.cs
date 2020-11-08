using System;
using ToSic.Eav.Documentation;

// Old stuff for compatibility with DNN, should not bleed into Oqtane
#if NET451
namespace ToSic.Eav.Data
{
    public partial class EntityDecorator
    {

        [PrivateApi]
        [Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
        public object GetBestValue(string attributeName, bool resolveHyperlinks)
            => Entity.GetBestValue(attributeName, resolveHyperlinks);

        [PrivateApi]
        [Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
        public object GetBestValue(string attributeName, string[] languages, bool resolveHyperlinks)
            => Entity.GetBestValue(attributeName, languages, resolveHyperlinks);

        [PrivateApi]
        [Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
        public T GetBestValue<T>(string attributeName, string[] languages, bool resolveHyperlinks)
            => Entity.GetBestValue<T>(attributeName, languages, resolveHyperlinks);

        [PrivateApi]
        [Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
        public object Value(string field, bool resolve) => Entity.Value(field, resolve);

        [PrivateApi]
        [Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
        public T Value<T>(string field, bool resolve) => Entity.Value<T>(field, resolve);

    }
}
#endif
