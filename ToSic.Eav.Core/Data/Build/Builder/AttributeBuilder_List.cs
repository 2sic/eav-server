using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Generics;
using static System.StringComparer;

namespace ToSic.Eav.Data.Build
{
    public partial class AttributeBuilder
    {
        public IImmutableDictionary<string, IAttribute> Empty() => EmptyList;
        private static readonly IImmutableDictionary<string, IAttribute> EmptyList = new Dictionary<string, IAttribute>().ToImmutableInvariant();

        public IImmutableDictionary<string, IAttribute> Create(IContentType contentType, ILookup<string, IValue> preparedValues)
        {
            var attributes = contentType.Attributes.ToImmutableDictionary(
                a => a.Name,
                a =>
                {
                    // It's important that we only get a list if there are values, otherwise we create empty lists
                    // which breaks other code
                    var values = preparedValues?.Contains(a.Name) == true
                        ? preparedValues[a.Name].ToList()
                        : null;
                    var entityAttribute = Create(a.Name, a.Type, values);
                    return entityAttribute;
                }, InvariantCultureIgnoreCase);
            return attributes;
        }


        public IImmutableDictionary<string, IAttribute> Create(IDictionary<string, IAttribute> attributes)
            => attributes?.ToImmutableInvariant() ?? Empty();

        public IImmutableDictionary<string, IAttribute> Create(IDictionary<string, object> attributes)
        {
            if (attributes == null)
                return Empty();

            if (attributes.All(x => x.Value is IAttribute))
                return attributes.ToImmutableDictionary(pair => pair.Key, pair => pair.Value as IAttribute, InvariantCultureIgnoreCase);

            return CreateDetailed(attributes).ToImmutableInvariant();
        }




        /// <summary>
        /// Convert a NameValueCollection-Like List to a Dictionary of IAttributes
        /// </summary>
        private Dictionary<string, IAttribute> CreateDetailed(IDictionary<string, object> objAttributes) =>
            objAttributes.ToDictionary(pair => pair.Key, oAttrib =>
            {
                // in case the object is already an IAttribute, use that, don't rebuild it
                if (oAttrib.Value is IAttribute typedValue)
                    return typedValue;

                // Not yet a proper IAttribute, construct from value
                var attributeType = DataTypes.GetAttributeTypeName(oAttrib.Value);
                var valuesModelList = new List<IValue>();
                if (oAttrib.Value != null)
                {
                    var valueModel = ValueBuilder.Build(attributeType, oAttrib.Value);
                    valuesModelList.Add(valueModel);
                }

                var attributeModel = Create(oAttrib.Key, attributeType, valuesModelList);

                return attributeModel;
            }, InvariantCultureIgnoreCase);

        #region Mutable operations - WIP

        public IDictionary<string, IAttribute> Mutable(IReadOnlyDictionary<string, IAttribute> attributes)
            => attributes?.ToDictionary(pair => pair.Key, pair => pair.Value, InvariantCultureIgnoreCase)
            ?? new Dictionary<string, IAttribute>(InvariantCultureIgnoreCase);

        public IDictionary<string, IAttribute> Replace(IDictionary<string, IAttribute> target, IAttribute newAttribute)
        {
            return new Dictionary<string, IAttribute>(target, InvariantCultureIgnoreCase)
            {
                [newAttribute.Name] = newAttribute
            };
        }

        #endregion

    }
}
