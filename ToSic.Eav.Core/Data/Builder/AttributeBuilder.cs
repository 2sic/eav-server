using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data.Builder
{
    public class AttributeBuilder
    {
        #region Dependency Injection

        public AttributeBuilder(Lazy<IValueConverter> lazyValueConverter)
        {
            _lazyValueConverter = lazyValueConverter;
        }

        private IValueConverter ValueConverter => _lazyValueConverter.Value;
        private readonly Lazy<IValueConverter> _lazyValueConverter;
        
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
            // pre-convert links if necessary...
            if (resolveHyperlink && valueType == ValueTypes.Hyperlink.ToString())
                value = ValueConverter.ToReference(valueType);

            // sometimes language is passed in as an empty string - this would have side effects, so it must be neutralized
            if (string.IsNullOrWhiteSpace(language)) language = null;

            var valueWithLanguages = ValueBuilder.Build(valueType, value, language == null
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

            return valueWithLanguages;
        }
        #endregion


        /// <summary>
        /// Get Attribute for specified Typ
        /// </summary>
        /// <returns><see cref="Attribute{ValueType}"/></returns>
        [PrivateApi("probably move to some attribute-builder or something")]
        public static IAttribute CreateTyped(string name, ValueTypes type, List<IValue> values = null)
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
        public static IAttribute CreateTyped(string name, string type, List<IValue> values = null)
            => CreateTyped(name, ValueTypeHelpers.Get(type), values);


    }
}
