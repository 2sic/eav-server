using System;
using ToSic.Eav.Context;
using ToSic.Eav.Documentation;

#if NETFRAMEWORK
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
        [Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
        public new object GetBestValue(string attributeName, bool resolveHyperlinks)
            => GetBestValue(attributeName, new string[0], resolveHyperlinks);


        [PrivateApi]
        [Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
        public TVal GetBestValue<TVal>(string name, string[] languages, bool resolveHyperlinks)
            => ChangeTypeOrDefault<TVal>(GetBestValue(name, languages, resolveHyperlinks));


        // 2020-12-15 2dm disabled, don't think it was ever in use
        //[PrivateApi]
        //[Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
        //public object Value(string field, bool resolve)
        //    => GetBestValue(field, new[] { IZoneCultureResolverExtensions.ThreadCultureNameNotGood() }, resolve);

        //[PrivateApi]
        //[Obsolete("Obsolete, was in DNN, shouldn't be supported any more - use overload without resolveHyperlink")]
        //public T Value<T>(string field, bool resolve)
        //    => ChangeTypeOrDefault<T>(GetBestValue(field, new[] { IZoneCultureResolverExtensions.ThreadCultureNameNotGood() }, resolve));

    }
}
#endif