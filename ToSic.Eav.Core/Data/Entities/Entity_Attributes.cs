﻿using System.Collections.Immutable;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {
        /// <inheritdoc />
        public IImmutableDictionary<string, IAttribute> Attributes { get; } //=> _attributes ?? (_attributes = _attributesRaw.ToImmutableInvariant());

        //private IImmutableDictionary<string, IAttribute> _attributes;

        [PrivateApi("WIP till immutable")]
        //private IDictionary<string, IAttribute> _attributesRaw { get; }

        //public IDictionary<string, IAttribute> _attributesForClone => _attributesRaw;

        /// <summary>
        /// This determines if the access to the properties will use light-objects, or IAttributes containing multi-language objects
        /// </summary>
        [PrivateApi("internal use only, can change any time")]
        public bool IsLight { get; }


        /// <inheritdoc />
        public new IAttribute this[string attributeName] => Attributes.TryGetValue(attributeName, out var result) ? result : null; 

    }
}
