﻿#if NETFRAMEWORK
using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {

        [PrivateApi]
        [Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
        public object GetBestValue(string attributeName, string[] languages, bool resolveHyperlinks)
        {
            if (_useLightModel)
                return base.GetBestValue(attributeName, resolveHyperlinks);

            var result = GetBestValueAndType(attributeName, languages, out var attributeType);

            if (attributeType == Constants.EntityFieldIsVirtual) return result;

            return resolveHyperlinks && attributeType == Constants.DataTypeHyperlink && result is string strResult
                ? TryToResolveLink(EntityGuid, strResult)
                : result;
        }

        [PrivateApi]
        [Obsolete("Deprecated. Do not use any more, as it cannot reliably know the real language list. Use GetBestValue(name, languageList)")]
        public new object GetBestValue(string attributeName) => GetBestValue(attributeName, new string[0]);

        // 2020-10-30 trying to drop uses with ResolveHyperlinks
        ///// <inheritdoc />
        //public new TVal GetBestValue<TVal>(string name, bool resolveHyperlinks/* = false*/)
        //    => ChangeTypeOrDefault<TVal>(GetBestValue(name, resolveHyperlinks));

        [PrivateApi]
        [Obsolete("Deprecated. Do not use any more, as it cannot reliably resolve hyperlinks.")]
        public new TVal GetBestValue<TVal>(string name) => ChangeTypeOrDefault<TVal>(GetBestValue(name));


        [PrivateApi]
        [Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
        public new object GetBestValue(string attributeName, bool resolveHyperlinks)
            => GetBestValue(attributeName, new string[0], resolveHyperlinks);


        [PrivateApi]
        [Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
        public TVal GetBestValue<TVal>(string name, string[] languages, bool resolveHyperlinks)
            => ChangeTypeOrDefault<TVal>(GetBestValue(name, languages, resolveHyperlinks));

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
        public T Value<T>(string field, bool resolve) => ChangeTypeOrDefault<T>(Value(field));

    }
}
#endif