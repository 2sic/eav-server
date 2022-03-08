using System.Collections.Generic;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Generics;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {
        /// <inheritdoc />
        public Dictionary<string, IAttribute> Attributes
        {
            get => _attributes ?? (_attributes = AttribBuilder.ConvertToInvariantDic(LightAttributesForInternalUseOnlyForNow));
            set => _attributes = (value ?? new Dictionary<string, IAttribute>()).ToInvariant();
        }

        private Dictionary<string, IAttribute> _attributes;

        /// <summary>
        /// This determines if the access to the properties will use light-objects, or IAttributes containing multi-language objects
        /// </summary>
        private bool _useLightModel;


        /// <inheritdoc />
        public new IAttribute this[string attributeName] => Attributes.ContainsKey(attributeName) ? Attributes[attributeName] : null;

    }
}
