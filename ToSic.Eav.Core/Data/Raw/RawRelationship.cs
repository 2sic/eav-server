﻿using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data.Raw
{
    /// <summary>
    /// Represents a relationship in a RawEntity.
    ///
    /// Basically you add such an object to your properties dictionary, containing the keys it needs to find its related items.
    /// </summary>
    /// <remarks>
    /// Added in 15.04
    /// </remarks>
    [PublicApi]
    public class RawRelationship : IRawRelationship
    {
        /// <summary>
        /// Create a raw relationship.
        /// </summary>
        /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
        /// <param name="key">A single key - if it's just a simple `string`, `int`, etc.</param>
        /// <param name="keys">A list of keys, if you have many.</param>
        public RawRelationship(
            string noParamOrder = Parameters.Protector,
            object key = default,
            IEnumerable<object> keys = default)
        {
            Parameters.Protect(noParamOrder);
            Keys = keys?.ToList()
                   ?? (key == null ? null : new List<object> { key })
                   ?? new List<object>();
        }

        /// <summary>
        /// The keys which will be used to find the related items.
        /// </summary>
        public List<object> Keys { get; }
    }
}