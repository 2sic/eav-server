using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.ImportExport.Convert
{
    /// <summary>
    /// Interface which converts one type into another, or a list of that type into a list of the resulting type.
    /// Commonly used to convert entities to dictionaries etc. 
    /// </summary>
    /// <typeparam name="TSource">The source format of this conversion</typeparam>
    /// <typeparam name="TConverted"></typeparam>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("Just FYI to understand the internals")]
    public interface IConvert<in TSource, out TConverted>
    {
        /// <summary>
        /// Return a list of converted objects - usually prepared for serialization or similar. 
        /// </summary>
        IEnumerable<TConverted> Convert(IEnumerable<TSource> list);

        /// <summary>
        /// Convert an object
        /// </summary>
        TConverted Convert(TSource item);
    }
}
