﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Data.Builder
{
    public static class AttribBuilder
    {


        #region Helper to assemble an entity from a dictionary of properties

        /// <summary>
        /// Convert a NameValueCollection-Like List to a Dictionary of IAttributes
        /// </summary>
        public static Dictionary<string, IAttribute> ConvertToInvariantDic(
            this IDictionary<string, object> objAttributes)
        {
            var result = new Dictionary<string, IAttribute>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var oAttrib in objAttributes)
            {
                // in case the object is already an IAttribute, use that, don't rebuild it
                if (oAttrib.Value is IAttribute typedValue)
                    result[oAttrib.Key] = typedValue;
                else
                {
                    var attributeType = GetAttributeTypeName(oAttrib.Value);
                    var attributeModel = AttributeBuilder.CreateTyped(oAttrib.Key, attributeType);
                    var valuesModelList = new List<IValue>();
                    if (oAttrib.Value != null)
                    {
                        var valueModel = ValueBuilder.Build(attributeType, oAttrib.Value, null);
                        valuesModelList.Add(valueModel);
                    }

                    attributeModel.Values = valuesModelList;

                    result[oAttrib.Key] = attributeModel;
                }


            }

            return result;


        }

        // helper to get text-name of the type
        public static string GetAttributeTypeName(object value)
        {
            if (value is DateTime)
                return Constants.DataTypeDateTime;
            if (value is decimal || value is int || value is double)
                return Constants.DataTypeNumber;
            if (value is bool)
                return Constants.DataTypeBoolean;
            if (value is Guid || value is List<Guid> || value is List<Guid?> || value is List<int> ||
                value is List<int?>)
                return Constants.DataTypeEntity;
            if (value is int[] || value is int?[])
                throw new Exception(
                    "Trying to provide an attribute with a value which is an int-array. This is not allowed - ask the iJungleboy.");
            return Constants.DataTypeString;
        }

        #endregion


        public static IAttribute CloneAttributeAndRename(IAttribute original, string newName)
        {
            var attributeType = GetAttributeTypeName(original);
            var newAttrib = AttributeBuilder.CreateTyped(newName, attributeType);
            newAttrib.Values = original.Values;
            return newAttrib;
        }


        public static Dictionary<string, IAttribute> Copy(this IDictionary<string, IAttribute> attributes)
            => attributes.ToDictionary(x => x.Key, x => x.Value.Copy());

#if NET451
        /// <summary>
        /// Add a value to the attribute specified. To do so, set the name, type and string of the value, as 
        /// well as some language properties.
        /// </summary>
        [Obsolete("You should now get the AttributeBuilder with Dependency Injection and AddValue there")]
        public static IValue AddValue(this Dictionary<string, IAttribute> target, string attributeName,
            object value, string valueType, string language = null, bool languageReadOnly = false,
            bool resolveHyperlink = false, IEntitiesSource allEntitiesForRelationships = null)
        {
            var builder = Factory.Resolve<AttributeBuilder>();
            return builder.AddValue(target, attributeName, value, valueType, language, languageReadOnly,
                resolveHyperlink, allEntitiesForRelationships);
        }
#endif

        ////#region Helper to add a value with languages to an existing list of Attributes
        /////// <summary>
        /////// Add a value to the attribute specified. To do so, set the name, type and string of the value, as 
        /////// well as some language properties.
        /////// </summary>
        ////public static IValue AddValue(this Dictionary<string, IAttribute> target, string attributeName,
        ////    object value, string valueType, string language = null, bool languageReadOnly = false,
        ////    bool resolveHyperlink = false, IEntitiesSource allEntitiesForRelationships = null, Lazy<IValueConverter> lazyValueConverter = null)
        ////{
        ////    // pre-convert links if necessary...
        ////    if (resolveHyperlink && valueType == ValueTypes.Hyperlink.ToString())
        ////    {
        ////        var valueConverter = lazyValueConverter?.Value ?? Factory.Resolve<IValueConverter>();
        ////        value = valueConverter.ToReference(valueType);
        ////    }

        ////    // sometimes language is passed in as an empty string - this would have side effects, so it must be neutralized
        ////    if (string.IsNullOrWhiteSpace(language)) language = null;

        ////    var valueWithLanguages = ValueBuilder.Build(valueType, value, language == null
        ////        ? null : new List<ILanguage> { new Language { Key = language, ReadOnly = languageReadOnly } }, allEntitiesForRelationships);


        ////    // add or replace to the collection
        ////    var attrExists = target.Where(item => item.Key == attributeName).Select(item => item.Value).FirstOrDefault();
        ////    if (attrExists == null)
        ////    {
        ////        var newAttr = AttributeBase.CreateTypedAttribute(attributeName, valueType, new List<IValue> { valueWithLanguages });
        ////        target.Add(attributeName, newAttr);
        ////    }
        ////    else
        ////    {
        ////        // todo: test if the new model has the same type as the attribute we're adding to
        ////        var attrib = target[attributeName];
        ////        // WIP: if(attrib.ControlledType != valueModel.)
        ////        // Now add...
        ////        attrib.Values.Add(valueWithLanguages);
        ////    }

        ////    return valueWithLanguages;
        ////}
        ////#endregion

        /// <summary>
        /// Get the value of an attribute in the language specified.
        /// </summary>
        public static IValue FindItemOfLanguage(this IDictionary<string, IAttribute> attributes, string key, string language)
        {
            var values = attributes
                .Where(item => item.Key == key)
                .Select(item => item.Value)
                .FirstOrDefault();
            return values?.Values.FirstOrDefault(value => value.Languages.Any(dimension => dimension.Key == language));
        }



        public static void BuildReferenceAttribute(this IEntity newEntity, string attribName, IEnumerable<int?> references,
            IEntitiesSource app)
        {
            var attrib = newEntity.Attributes[attribName];
            attrib.Values = new List<IValue> { ValueBuilder.Build(attrib.Type, references, null, app) };
        }


        public static void FixIncorrectLanguageDefinitions(this IAttribute attrib, string primaryLanguage)
        {
            // Background: there are rare cases, where data was stored incorrectly
            // this happens when a attribute has multiple values, but some don't have languages assigned
            // that would be invalid, as any property with a language code must have all the values (for that property) with language codes
            if (attrib.Values.Count > 1 && attrib.Values.Any(v => !v.Languages.Any()))
            {
                var badValuesWithoutLanguage = attrib.Values.Where(v => !v.Languages.Any()).ToList();
                if (!badValuesWithoutLanguage.Any()) return;

                // new 2020-11-12 We sometimes ran into old data which had this problem
                // but since the primary language was the missing one, this caused a lot of follow up
                // so no we want to check if the primary language is missing - and if yes, assign that
                var hasPrimary = attrib.Values.Any(v => v.Languages.Any(l => l.Key == primaryLanguage));

                // only attach the primary language to a value if we don't already have a primary value
                if (!hasPrimary)
                {
                    var firstWithoutLanguage = badValuesWithoutLanguage.First();
                    firstWithoutLanguage.Languages.Add(new Language
                    {
                        DimensionId = 0, // unknown - should be fine...
                        Key = primaryLanguage,
                        ReadOnly = false
                    });

                    // Skip the modified item and check if we still have any to remove
                    badValuesWithoutLanguage.Remove(firstWithoutLanguage);
                }

                if (badValuesWithoutLanguage.Any())
                    badValuesWithoutLanguage.ForEach(badValue =>
                        attrib.Values.Remove(badValue));
            }
        }
    }
}
