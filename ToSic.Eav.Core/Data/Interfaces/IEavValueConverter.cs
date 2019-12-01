using System;

namespace ToSic.Eav.Implementations.ValueConverter
{
    public interface IEavValueConverter
    {
        /// <summary>
        /// Resolve a value to a reference which is managed by the environment
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        string ToReference(string value);

        /// <summary>
        /// Resolve a reference to a value using the environment resolver
        /// </summary>
        /// <param name="itemGuid">Guid of this item</param>
        /// <param name="reference">Reference code (or something else) - if not a code, will not resolve</param>
        /// <returns></returns>
        string ToValue(Guid itemGuid, string reference);
    }
}
