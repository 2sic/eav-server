using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Run;

namespace ToSic.Eav.Data.Builder
{
    public class AttributeBuilder
    {
        private readonly Lazy<IValueConverter> _lazyValueConverter;
        private IValueConverter ValueConverter => _lazyValueConverter.Value;

        public AttributeBuilder(Lazy<IValueConverter> lazyValueConverter)
        {
            _lazyValueConverter = lazyValueConverter;
        }

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
            {
                var valueConverter = ValueConverter;
                value = valueConverter.ToReference(valueType);
            }

            // sometimes language is passed in as an empty string - this would have side effects, so it must be neutralized
            if (string.IsNullOrWhiteSpace(language)) language = null;

            var valueWithLanguages = ValueBuilder.Build(valueType, value, language == null
                ? null : new List<ILanguage> { new Language { Key = language, ReadOnly = languageReadOnly } }, allEntitiesForRelationships);


            // add or replace to the collection
            var attrExists = target.Where(item => item.Key == attributeName).Select(item => item.Value).FirstOrDefault();
            if (attrExists == null)
            {
                var newAttr = AttributeBase.CreateTypedAttribute(attributeName, valueType, new List<IValue> { valueWithLanguages });
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
    }
}
