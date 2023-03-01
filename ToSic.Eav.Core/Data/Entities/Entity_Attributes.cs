using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {
        /// <inheritdoc />
        public IReadOnlyDictionary<string, IAttribute> Attributes => _attributes ?? (_attributes = _attributesRaw.ToImmutableDictionary(StringComparer.InvariantCultureIgnoreCase));

        private IReadOnlyDictionary<string, IAttribute> _attributes;

        [PrivateApi("WIP till immutable")]
        public readonly IDictionary<string, IAttribute> _attributesRaw;

        /// <summary>
        /// Temporary workaround as we're making the Attributes immutable
        /// </summary>
        /// <param name="mutator"></param>
        [PrivateApi]
        public void MutateAttributes(Action<IDictionary<string, IAttribute>> mutator)
        {
            mutator(_attributesRaw);
            _attributes = null;
        }

        /// <summary>
        /// This determines if the access to the properties will use light-objects, or IAttributes containing multi-language objects
        /// </summary>
        [PrivateApi("internal use only, can change any time")]
        public bool IsLight { get; }


        /// <inheritdoc />
        public new IAttribute this[string attributeName] => Attributes.TryGetValue(attributeName, out var result) ? result : null; 

    }
}
