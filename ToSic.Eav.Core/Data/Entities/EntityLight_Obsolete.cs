using System;
using ToSic.Eav.Documentation;

#if NETFRAMEWORK
namespace ToSic.Eav.Data
{
    public partial class EntityLight
    {
        /// <inheritdoc />
        [Obsolete("Deprecated. Do not use any more, as it cannot reliably resolve hyperlinks.")]
        [PrivateApi]
        public object GetBestValue(string attributeName, bool resolveHyperlinks)
        {
            var result = GetBestValue(attributeName);

            if (resolveHyperlinks && result is string strResult)
                result = TryToResolveLink(EntityGuid, strResult);

            return result;
        }

        [Obsolete("Deprecated. Do not use any more, as it cannot reliably resolve hyperlinks.")]
        [PrivateApi]
        protected static string TryToResolveLink(Guid itemGuid, string result)
        {
            if (!ValueConverterBase.CouldBeReference(result)) return result;
            return Factory.Resolve<IValueConverter>().ToValue(result, itemGuid);
        }


        ///// <inheritdoc />
        //public TVal GetBestValue<TVal>(string name, bool resolveHyperlinks) 
        //    => ChangeTypeOrDefault<TVal>(GetBestValue(name, resolveHyperlinks));

    }
}
#endif