using System.Collections.Generic;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {
        /// <inheritdoc />
        public IDictionary<string, IAttribute> Attributes { get; }

        /// <summary>
        /// This determines if the access to the properties will use light-objects, or IAttributes containing multi-language objects
        /// </summary>
        [PrivateApi("internal use only, can change any time")]
        public bool IsLight { get; }


        /// <inheritdoc />
        public new IAttribute this[string attributeName] => Attributes.TryGetValue(attributeName, out var result) ? result : null; 

    }
}
