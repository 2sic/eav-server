using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;

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

        public IEntity AddValueWIP(IEntity entity, string attributeName,
            object value, string valueType, string language = null, bool languageReadOnly = false,
            bool resolveHyperlink = false, IEntitiesSource allEntitiesForRelationships = null,
            ILanguage additionalLanguageWip =
                default // 2023-02-28 2dm - added this for an edge case, should be cleaned up some day
        )
        {
            var temp = AddValue((entity as Entity)._attributesRaw, attributeName, value, valueType, language,
                languageReadOnly, resolveHyperlink, allEntitiesForRelationships, additionalLanguageWip);
            return entity;
        }


        #region Helper to add a value with languages to an existing list of Attributes
        /// <summary>
        /// Add a value to the attribute specified. To do so, set the name, type and string of the value, as 
        /// well as some language properties.
        /// </summary>
        public IValue AddValue(IDictionary<string, IAttribute> target, string attributeName,
            object value, string valueType, string language = null, bool languageReadOnly = false,
            bool resolveHyperlink = false, IEntitiesSource allEntitiesForRelationships = null,
            ILanguage additionalLanguageWip = default // 2023-02-28 2dm - added this for an edge case, should be cleaned up some day
            )
        {
            var wrapLog = Log.Fn<IValue>($"..., {attributeName}, {value} ({valueType}), {language}, ..., {nameof(resolveHyperlink)}: {resolveHyperlink}");
            // pre-convert links if necessary...
            if (resolveHyperlink && valueType == ValueTypes.Hyperlink.ToString() && value is string stringValue)
            {
                Log.A($"Will resolve hyperlink for '{stringValue}'");
                value = _valueConverter.Value.ToReference(stringValue);
                Log.A($"New value: '{stringValue}'");
            }

            // sometimes language is passed in as an empty string - this would have side effects, so it must be neutralized
            if (string.IsNullOrWhiteSpace(language)) language = null;

            var valueLanguages = language == null
                ? null // must be null if no languages are specified, to cause proper fallback
                // 2023-02-24 2dm #immutable
                //: new List<ILanguage> { new Language { Key = language, ReadOnly = languageReadOnly } }, allEntitiesForRelationships);
                : new List<ILanguage> { new Language(language, languageReadOnly) };

            // 2023-02-28 2dm #immutable workaround for now
            if (additionalLanguageWip != null)
            {
                valueLanguages = valueLanguages ?? new List<ILanguage>();
                valueLanguages.Add(additionalLanguageWip);
            }

            var valueWithLanguages = ValueBuilder.Build(valueType, value, valueLanguages?.ToImmutableList(), allEntitiesForRelationships);


            // add or replace to the collection
            var attrExists = target
                .Where(item => item.Key == attributeName)
                .Select(item => item.Value)
                .FirstOrDefault();

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

            return wrapLog.Return(valueWithLanguages);
        }
        #endregion
    }
}
