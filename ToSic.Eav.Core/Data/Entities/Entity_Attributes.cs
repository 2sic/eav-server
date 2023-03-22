using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {
        /// <inheritdoc />
        public IImmutableDictionary<string, IAttribute> Attributes => _attributes;
        private readonly IImmutableDictionary<string, IAttribute> _attributes;

        /// <inheritdoc />
        public new IAttribute this[string attributeName] => Attributes.TryGetValue(attributeName, out var result) ? result : null; 

    }
}
