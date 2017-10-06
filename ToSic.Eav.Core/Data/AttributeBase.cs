using System;
using System.Collections.Generic;
using ToSic.Eav.Enums;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <inheritdoc />
    /// <summary>
    /// Represents an Attribute with Values of a Generic Type
    /// </summary>
    public class AttributeBase : IAttributeBase
    {
        //public int TempAppId { get; }
        public string Name { get; set; }
        public string Type { get; set; }

        private AttributeTypeEnum _controlledType = AttributeTypeEnum.Undefined;

        public AttributeTypeEnum ControlledType
        {
            get => _controlledType != AttributeTypeEnum.Undefined 
                ? _controlledType
                : _controlledType = ParseToAttributeTypeEnum(Type);
            internal set => _controlledType = value;
        }

        private static AttributeTypeEnum ParseToAttributeTypeEnum(string typeName)
        {
            // if the type has not been set yet, try to look it up...
            if (typeName != null && Enum.IsDefined(typeof(AttributeTypeEnum), typeName))
                return (AttributeTypeEnum) Enum.Parse(typeof(AttributeTypeEnum), typeName);
            return AttributeTypeEnum.Undefined;
        }

        /// <summary>
        /// Extended constructor when also storing the persistance ID-Info
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public AttributeBase(string name, string type)
        {
            Name = name;
            Type = type;
		}

        /// <summary>
        /// Get Attribute for specified Typ
        /// </summary>
        /// <returns><see cref="Attribute{ValueType}"/></returns>
        public static IAttribute CreateTypedAttribute(string name, AttributeTypeEnum type, List<IValue> values = null)
        {
            var typeName = type.ToString();
            var result = ((Func<IAttribute>)(() => { 
            switch (type)
            {
                case AttributeTypeEnum.Boolean:
                    return new Attribute<bool?>(name, typeName);
                case AttributeTypeEnum.DateTime:
                    return new Attribute<DateTime?>(name, typeName);
                case AttributeTypeEnum.Number:
                    return new Attribute<decimal?>(name, typeName);
                case AttributeTypeEnum.Entity:
                    return new Attribute<EntityRelationship>(name, typeName) { Values = new List<IValue> { Value.NullRelationship } };
                // ReSharper disable RedundantCaseLabel
                case AttributeTypeEnum.String:
                case AttributeTypeEnum.Hyperlink:
                case AttributeTypeEnum.Custom:
                case AttributeTypeEnum.Undefined:
                case AttributeTypeEnum.Empty:
                // ReSharper restore RedundantCaseLabel
                default:
                    return new Attribute<string>(name, typeName);
            }
            }))();
            if (values != null)
                result.Values = values;

            return result;
        }

        public static IAttribute CreateTypedAttribute(string name, string type, List<IValue> values = null) 
            => CreateTypedAttribute(name, ParseToAttributeTypeEnum(type), values);


        

    }
}