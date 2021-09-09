using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Interface which converts one type into another. Commonly used to convert entities to dictionaries etc. 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TConverted"></typeparam>
    [PublicApi]
    public interface IConvert<TSource, out TConverted>
    {
        /// <summary>
        /// Return a list of converted objects - usually prepared for serialization or similar. 
        /// </summary>
        IEnumerable<TConverted> Convert(IEnumerable<TSource> dynamicList);

        /// <summary>
        /// Convert an object
        /// </summary>
        TConverted Convert(TSource dynamicEntity);
    }
}
