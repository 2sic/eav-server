using System;
using ToSic.Eav.Documentation;

#if NET451
namespace ToSic.Eav.Data
{
    public partial class EntityLight
    {
        /// <inheritdoc />
        public object GetBestValue(string attributeName, bool resolveHyperlinks)
        {
            var result = GetBestValue(attributeName);

            if (resolveHyperlinks && result is string strResult)
                result = TryToResolveLink(EntityGuid, strResult);

            return result;
        }

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