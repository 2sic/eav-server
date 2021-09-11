using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Convert
{
    /// <summary>
    /// Interface which converts one type into another, or a list of that type into a list of the resulting type.
    /// Commonly used to convert entities to dictionaries etc. 
    /// </summary>
    /// <typeparam name="TFrom">The source format of this conversion</typeparam>
    /// <typeparam name="TTo"></typeparam>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("Just FYI to understand the internals. It will probably be moved elsewhere in the namespace some day. ")]
    public interface IConvert<in TFrom, out TTo>
    {
        /// <summary>
        /// Return a list of converted objects - usually prepared for serialization or similar. 
        /// </summary>
        IEnumerable<TTo> Convert(IEnumerable<TFrom> list);

        /// <summary>
        /// Convert an object
        /// </summary>
        TTo Convert(TFrom item);
    }
}
