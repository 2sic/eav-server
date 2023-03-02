using System.Collections.Immutable;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {
        /// <inheritdoc />
        public IImmutableDictionary<string, IAttribute> Attributes { get; }


        /// <inheritdoc />
        public new IAttribute this[string attributeName] => Attributes.TryGetValue(attributeName, out var result) ? result : null; 

    }
}
