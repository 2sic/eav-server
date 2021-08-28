#if NETFRAMEWORK
using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    public partial class EntityLight
    {
        /// <inheritdoc />
        [PrivateApi]
        [Obsolete("Deprecated. Do not use any more, as it cannot reliably resolve hyperlinks.")]
        public object GetBestValue(string attributeName, bool resolveHyperlinks)
        {
            var result = GetBestValue(attributeName);

            if (resolveHyperlinks && result is string strResult)
                result = TryToResolveLink(EntityGuid, strResult);

            return result;
        }

        [PrivateApi]
        [Obsolete("Deprecated. Do not use any more, as it cannot reliably resolve hyperlinks.")]
        protected static string TryToResolveLink(Guid itemGuid, string result)
        {
            if (!ValueConverterBase.CouldBeReference(result)) return result;
            return Factory.StaticBuild<IValueConverter>().ToValue(result, itemGuid);
        }
    }
}
#endif