using System;
using System.Collections.Generic;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <inheritdoc />
    /// <summary>
    /// Represents an Attribute with Values of a Generic Type
    /// </summary>
    public class AttributeBase : IAttributeBase
    {
        public string Name { get; set; }
        public string Type { get; set; }

        private ValueTypes _controlledType = ValueTypes.Undefined;

        public ValueTypes ControlledType
        {
            get => _controlledType != ValueTypes.Undefined 
                ? _controlledType
                : _controlledType = ParseToAttributeTypeEnum(Type);
            internal set => _controlledType = value;
        }

        private static ValueTypes ParseToAttributeTypeEnum(string typeName)
        {
            // if the type has not been set yet, try to look it up...
            if (typeName != null && Enum.IsDefined(typeof(ValueTypes), typeName))
                return (ValueTypes) Enum.Parse(typeof(ValueTypes), typeName);
            return ValueTypes.Undefined;
        }

        /// <summary>
        /// Extended constructor when also storing the persistance ID-Info
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        [PrivateApi]
        public AttributeBase(string name, string type)
        {
            Name = name;
            Type = type;
		}

        /// <summary>
        /// Get Attribute for specified Typ
        /// </summary>
        /// <returns><see cref="Attribute{ValueType}"/></returns>
        [PrivateApi("probably move to some attribute-builder or something")]
        public static IAttribute CreateTypedAttribute(string name, ValueTypes type, List<IValue> values = null)
        {
            var typeName = type.ToString();
            var result = ((Func<IAttribute>)(() => { 
            switch (type)
            {
                case ValueTypes.Boolean:
                    return new Attribute<bool?>(name, typeName);
                case ValueTypes.DateTime:
                    return new Attribute<DateTime?>(name, typeName);
                case ValueTypes.Number:
                    return new Attribute<decimal?>(name, typeName);
                case ValueTypes.Entity:
                    return new Attribute<IEnumerable<IEntity>>(name, typeName) { Values = new List<IValue> { ValueBuilder.NullRelationship } };
                // ReSharper disable RedundantCaseLabel
                case ValueTypes.String:
                case ValueTypes.Hyperlink:
                case ValueTypes.Custom:
                case ValueTypes.Undefined:
                case ValueTypes.Empty:
                // ReSharper restore RedundantCaseLabel
                default:
                    return new Attribute<string>(name, typeName);
            }
            }))();
            if (values != null)
                result.Values = values;

            return result;
        }

        [PrivateApi]
        public static IAttribute CreateTypedAttribute(string name, string type, List<IValue> values = null) 
            => CreateTypedAttribute(name, ParseToAttributeTypeEnum(type), values);

    }
}