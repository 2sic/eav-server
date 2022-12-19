using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data.Builder
{
    public class AttributeBuilder: HasLog
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


        public Dictionary<string, IAttribute> Clone(IDictionary<string, IAttribute> attributes) 
            => attributes?.ToDictionary(x => x.Key, x => Clone(x.Value));

        public IAttribute Clone(IAttribute original)
            => CreateTyped(original.Name, original.Type, original.Values.Select(v => ValueBuilder.Clone(v, original.Type)).ToList());


        /// <summary>
        /// Get Attribute for specified Typ
        /// </summary>
        /// <returns><see cref="Attribute{ValueType}"/></returns>
        [PrivateApi("probably move to some attribute-builder or something")]
        public static IAttribute CreateTyped(string name, ValueTypes type, IList<IValue> values = null)
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
                        return new Attribute<IEnumerable<IEntity>>(name, typeName) { Values = new List<IValue> { new ValueBuilder(new DimensionBuilder()).NullRelationship } };
                    // ReSharper disable RedundantCaseLabel
                    case ValueTypes.String:
                    case ValueTypes.Hyperlink:
                    case ValueTypes.Custom:
                    case ValueTypes.Json:
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

        public IAttribute GenerateAttributesOfContentType(IEntity newEntity, IContentType contentType)
        {
            IAttribute titleAttrib = null;
            foreach (var definition in contentType.Attributes)
            {

                var entityAttribute = CreateTyped(definition.Name, definition.Type);
                newEntity.Attributes.Add(entityAttribute.Name, entityAttribute);
                if (definition.IsTitle)
                    titleAttrib = entityAttribute;
            }
            return titleAttrib;
        }


        [PrivateApi]
        public IAttribute CreateTyped(string name, string type, IList<IValue> values = null)
            => CreateTyped(name, ValueTypeHelpers.Get(type), values);


    }
}
