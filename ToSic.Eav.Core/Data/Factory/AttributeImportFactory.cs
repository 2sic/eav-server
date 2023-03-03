using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using ToSic.Eav.Generics;
using ToSic.Eav.Plumbing;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using static System.StringComparer;

namespace ToSic.Eav.Data.Builder
{
    public class AttributeBuilderForImport: AttributeBuilder
    {
        public AttributeBuilderForImport(LazySvc<IValueConverter> valueConverter, ValueBuilder valueBuilder) : base(valueBuilder)
        {
            ConnectServices(
                _valueConverter = valueConverter
            );
        }
        private readonly LazySvc<IValueConverter> _valueConverter;


        #region Helper to add a value with languages to an existing list of Attributes
        /// <summary>
        /// Add a value to the attribute specified. To do so, set the name, type and string of the value, as 
        /// well as some language properties.
        /// </summary>
        public IAttribute CreateAttribute(IDictionary<string, IAttribute> target, string attributeName,
            object value, ValueTypes valueType, string language = null, bool languageReadOnly = false,
            IEntitiesSource allEntitiesForRelationships = null
        ) => Log.Func($"..., {attributeName}, {value} ({valueType}), {language}, ...", l =>
        {
            var valueLanguages = GetBestValueLanguages(language, languageReadOnly);

            var valueWithLanguages = ValueBuilder.Build(valueType, value, valueLanguages?.ToImmutableList(), allEntitiesForRelationships);


            // add or replace to the collection
            var exists = target.TryGetValue(attributeName, out var existingAttribute);

            if (!exists)
                return CreateTyped(attributeName, valueType, new List<IValue> { valueWithLanguages });
            
            // maybe: test if the new model has the same type as the attribute we're adding to
            // WIP: if(attrib.ControlledType != valueModel.)
            // Now add...
            //var attribModifyiable = existingAttribute.Values.ToList();
            //attribModifyiable.Add(valueWithLanguages);
            var updatedValueList = ReplaceValue(existingAttribute.Values, null, valueWithLanguages);
            return existingAttribute.CloneWithNewValues(updatedValueList);
            //attrib.Values.Add(valueWithLanguages);
        });

        public IDictionary<string, IAttribute> UpdateAttribute(IDictionary<string, IAttribute> target,
            IAttribute newAttribute)
        {
            return new Dictionary<string, IAttribute>(target, InvariantCultureIgnoreCase)
            {
                [newAttribute.Name] = newAttribute
            };
        }

        public object PreConvertReferences(object value, ValueTypes valueType, bool resolveHyperlink) => Log.Func(() =>
        {
            if (resolveHyperlink && valueType == ValueTypes.Hyperlink && value is string stringValue)
            {
                var converted = _valueConverter.Value.ToReference(stringValue);
                return (converted, $"Resolve hyperlink for '{stringValue}' - New value: '{converted}'") ;
            }

            return (value, "unmodified");
        });

        public List<ILanguage> GetBestValueLanguages(string language, bool languageReadOnly)
        {
            // sometimes language is passed in as an empty string - this would have side effects, so it must be neutralized
            if (string.IsNullOrWhiteSpace(language)) language = null;

            var valueLanguages = language == null
                ? null // must be null if no languages are specified, to cause proper fallback
                // 2023-02-24 2dm #immutable
                //: new List<ILanguage> { new Language { Key = language, ReadOnly = languageReadOnly } }, allEntitiesForRelationships);
                : new List<ILanguage> { new Language(language, languageReadOnly) };

            return valueLanguages;
        }

        // todo: possibly move to a valuebuilder or something
        public IValue UpdateLanguages(IValue original, List<ILanguage> updateLanguages)
        {
            var languages = original.Languages.ToList();
            // loop through original to ensure we don't modify the order
            languages = languages
                .Select(l => updateLanguages.FirstOrDefault(ul => ul.Key.EqualsInsensitive(l.Key)) ?? l)
                .ToList();
            var rest = updateLanguages.Where(ul => !languages.Any(l => l.Key.EqualsInsensitive(ul.Key)));
            var final = languages.Concat(rest).ToImmutableList();
            var clonedValue = original.Clone(final);
            return clonedValue;
        }

        public IImmutableList<IValue> ReplaceValue(IReadOnlyList<IValue> values, IValue oldValue, IValue newValue)
        {
            var editable = values.ToList();
            // note: should preserve order
            var index = editable.IndexOf(oldValue);
            if (index == -1)
                editable.Add(newValue);
            else
                editable[index] = newValue;
            return editable.ToImmutableList();
        }

        #endregion
    }
}
