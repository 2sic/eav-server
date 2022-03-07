using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Data.Builder
{
    public class AttributeBuilder: HasLog<AttributeBuilder>
    {
        #region Dependency Injection

        public AttributeBuilder(
            Lazy<IValueConverter> valueConverter,
            Lazy<ValueBuilder> valueBuilder
        ): base("Dta.AttBld")
        {
            _valueConverter = valueConverter;
            _valueBuilder = valueBuilder;
        }
        private readonly Lazy<IValueConverter> _valueConverter;
        private readonly Lazy<ValueBuilder> _valueBuilder;

        #endregion

        #region Helper to add a value with languages to an existing list of Attributes
        /// <summary>
        /// Add a value to the attribute specified. To do so, set the name, type and string of the value, as 
        /// well as some language properties.
        /// </summary>
        public IValue AddValue(Dictionary<string, IAttribute> target, string attributeName,
            object value, string valueType, string language = null, bool languageReadOnly = false,
            bool resolveHyperlink = false, IEntitiesSource allEntitiesForRelationships = null)
        {
            var wrapLog = Log.Call<IValue>($"..., {attributeName}, {value} ({valueType}), {language}, ..., {nameof(resolveHyperlink)}: {resolveHyperlink}");
            // pre-convert links if necessary...
            if (resolveHyperlink && valueType == ValueTypes.Hyperlink.ToString() && value is string stringValue)
            {
                Log.Add($"Will resolve hyperlink for '{stringValue}'");
                value = _valueConverter.Value.ToReference(stringValue);
                Log.Add($"New value: '{stringValue}'");
            }

            // sometimes language is passed in as an empty string - this would have side effects, so it must be neutralized
            if (string.IsNullOrWhiteSpace(language)) language = null;

            var valueWithLanguages = _valueBuilder.Value.Build(valueType, value, language == null
                ? null : new List<ILanguage> { new Language { Key = language, ReadOnly = languageReadOnly } }, allEntitiesForRelationships);


            // add or replace to the collection
            var attrExists = target.Where(item => item.Key == attributeName).Select(item => item.Value).FirstOrDefault();
            if (attrExists == null)
            {
                var newAttr = CreateTyped(attributeName, valueType, new List<IValue> { valueWithLanguages });
                target.Add(attributeName, newAttr);
            }
            else
            {
                // todo: test if the new model has the same type as the attribute we're adding to
                var attrib = target[attributeName];
                // WIP: if(attrib.ControlledType != valueModel.)
                // Now add...
                attrib.Values.Add(valueWithLanguages);
            }

            return wrapLog(null, valueWithLanguages);
        }
        #endregion

        /// <summary>
        /// Create a reference / relationship attribute on an entity being constructed (at DB load)
        /// </summary>
        public void BuildReferenceAttribute(IEntity newEntity, string attribName, IEnumerable<int?> references,
            IEntitiesSource app)
        {
            var attrib = newEntity.Attributes[attribName];
            attrib.Values = new List<IValue> { _valueBuilder.Value.Build(attrib.Type, references, null, app) };
        }


        public Dictionary<string, IAttribute> Clone(IDictionary<string, IAttribute> attributes) 
            => attributes?.ToDictionary(x => x.Key, x => Clone(x.Value));

        public IAttribute Clone(IAttribute original)
            => CreateTyped(original.Name, original.Type, original.Values.Select(v => _valueBuilder.Value.Clone(v, original.Type)).ToList());


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
                        return new Attribute<IEnumerable<IEntity>>(name, typeName) { Values = new List<IValue> { ValueBuilder.NullRelationship } };
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

        [PrivateApi]
        public static IAttribute CreateTyped(string name, string type, IList<IValue> values = null)
            => CreateTyped(name, ValueTypeHelpers.Get(type), values);


    }
}
