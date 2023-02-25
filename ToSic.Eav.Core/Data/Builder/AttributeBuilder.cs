using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;
using static System.StringComparer;

namespace ToSic.Eav.Data.Builder
{
    public class AttributeBuilder: ServiceBase
    {
        #region Dependency Injection

        public AttributeBuilder(ValueBuilder valueBuilder): base("Dta.AttBld")
        {
            ValueBuilder = valueBuilder;
        }
        protected readonly ValueBuilder ValueBuilder;

        #endregion


        /// <summary>
        /// Create a reference / relationship attribute on an entity being constructed (at DB load)
        /// </summary>
        public void BuildReferenceAttribute(IEntity newEntity, string attribName, IEnumerable<int?> references,
            IEntitiesSource app)
        {
            var attrib = newEntity.Attributes[attribName];
            attrib.Values = new List<IValue> { ValueBuilder.Build(attrib.Type, references, null, app) };
        }

        public IDictionary<string, IAttribute> ListRemoveOne(IDictionary<string, IAttribute> list, string keyToDrop)
            => list.Where(a => !a.Key.EqualsInsensitive(keyToDrop))
                .ToDictionary(pair => pair.Key, pair => pair.Value, InvariantCultureIgnoreCase);

        public IDictionary<string, IAttribute> ListAddOne(IDictionary<string, IAttribute> list, string name, IAttribute fieldToAdd) 
            => new Dictionary<string, IAttribute>(list, InvariantCultureIgnoreCase) { { name, fieldToAdd } };

        public IDictionary<string, IAttribute> ListUpdateOne(IDictionary<string, IAttribute> list, IAttribute field,
            IList<IValue> values)
        {
            var copy = new Dictionary<string, IAttribute>(list, InvariantCultureIgnoreCase)
            {
                [field.Name] = CloneUpdateOne(field, values)
            };
            return copy;
        }

        // Note: ATM it makes a deep clone, but once everything is #immutable that won't be necessary any more
        public Dictionary<string, IAttribute> ListDeepClone(IDictionary<string, IAttribute> attributes) 
            => attributes?.ToDictionary(pair => pair.Key, pair => CloneUpdateOne(pair.Value), InvariantCultureIgnoreCase);

        public IAttribute CloneUpdateOne(IAttribute original, IList<IValue> values = null)
            => CreateTyped(original.Name, original.Type,
                values ?? original.Values
                    .Select(v => ValueBuilder.Clone(v, original.Type))
                    .ToList()
            );

        [PrivateApi]
        public IAttribute CreateTyped(string name, string type, IList<IValue> values = null)
            => CreateTyped(name, ValueTypeHelpers.Get(type), values);

        /// <summary>
        /// Get Attribute for specified Typ
        /// </summary>
        /// <returns><see cref="Attribute{ValueType}"/></returns>
        [PrivateApi("probably move to some attribute-builder or something")]
        public IAttribute CreateTyped(string name, ValueTypes type, IList<IValue> values = null)
        {
            var typeName = type.ToString();
            switch (type)
            {
                case ValueTypes.Boolean:
                    return new Attribute<bool?>(name, typeName, values);
                case ValueTypes.DateTime:
                    return new Attribute<DateTime?>(name, typeName, values);
                case ValueTypes.Number:
                    return new Attribute<decimal?>(name, typeName, values);
                case ValueTypes.Entity:
                    return new Attribute<IEnumerable<IEntity>>(name, typeName,
                        // Note 2023-02-24 2dm - up until now the values were never used
                        // in this case, so relationships created here were always empty
                        // Could break something, but I don't think it will
                        values 
                        ?? new List<IValue> { ValueBuilder.NewEmptyRelationship });
                // ReSharper disable RedundantCaseLabel
                case ValueTypes.String:
                case ValueTypes.Hyperlink:
                case ValueTypes.Custom:
                case ValueTypes.Json:
                case ValueTypes.Undefined:
                case ValueTypes.Empty:
                // ReSharper restore RedundantCaseLabel
                default:
                    return new Attribute<string>(name, typeName, values);
            }
        }

        public (Dictionary<string, IAttribute> All, string Title) GenerateAttributesOfContentType(IContentType contentType)
        {
            string titleField = null;
            var attributes = contentType.Attributes.ToDictionary(
                a => a.Name,
                a =>
                {
                    var entityAttribute = CreateTyped(a.Name, a.Type);
                    if (a.IsTitle)
                        titleField = entityAttribute.Name;
                    return entityAttribute;
                });
            return (attributes, titleField);
        }


        /// <summary>
        /// Convert a NameValueCollection-Like List to a Dictionary of IAttributes
        /// </summary>
        public Dictionary<string, IAttribute> ConvertToIAttributeDic(IDictionary<string, object> objAttributes) =>
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

                var attributeModel = CreateTyped(oAttrib.Key, attributeType, valuesModelList);

                return attributeModel;
            });
    }
}
