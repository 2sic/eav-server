using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Marks objects that can convert values - like "file:22" to "/images/logo.jpg" and back.
    /// </summary>
    [PublicApi]
    public interface IValueConverter
    {
        /// <summary>
        /// Resolve a value to a reference which is managed by the environment
        /// </summary>
        /// <param name="value">the full value, like "image/logo.jpg"</param>
        /// <returns>The reference, like "file:22".</returns>
        string ToReference(string value);

        /// <summary>
        /// Resolve a reference to a value using the environment resolver
        /// </summary>
        /// <param name="itemGuid">
        /// Guid of the item/entity which was using the reference. <br/>
        /// The Guid is used when security setting only allow resolving within the own item.
        /// This ensures that external requests cannot just number through all possible IDs.
        /// </param>
        /// <param name="reference">Reference code (or something else) - if not a code, will not resolve</param>
        /// <returns>The value, like the url.</returns>
        string ToValue(Guid itemGuid, string reference);
    }
}
