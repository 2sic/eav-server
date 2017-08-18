using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Enums;
using ToSic.Eav.Implementations.ValueConverter;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data.Builder
{
    public static class AttribBuilder
    {


        #region Helper to assemble an entity from a dictionary of properties
        /// <summary>
        /// Convert a NameValueCollection-Like List to a Dictionary of IAttributes
        /// </summary>
        public static Dictionary<string, IAttribute> ConvertToAttributes(this IDictionary<string, object> objAttributes)
        {
            var result = new Dictionary<string, IAttribute>(StringComparer.OrdinalIgnoreCase);

            foreach (var oAttrib in objAttributes)
            {
                // in case the object is already an IAttribute, use that, don't rebuild it
                if (oAttrib.Value is IAttribute)
                    result[oAttrib.Key] = (IAttribute)oAttrib.Value;
                else
                {
                    var attributeType = GetAttributeTypeName(oAttrib.Value);
                    //var baseModel = new AttributeDefinition(attribute.Key, attributeType, attribute.Key == titleAttributeName, 0, 0);
                    var attributeModel = AttributeBase.CreateTypedAttribute(oAttrib.Key, attributeType);//  baseModel.CreateAttribute();
                    var valuesModelList = new List<IValue>();
                    if (oAttrib.Value != null)
                    {
                        var valueModel = Value.Build(attributeType, oAttrib.Value, null);
                        valuesModelList.Add(valueModel);
                    }

                    attributeModel.Values = valuesModelList;

                    result[oAttrib.Key] = attributeModel;
                }


            }

            return result;

            // helper to get text-name of the type
            string GetAttributeTypeName(object value)
            {
                if (value is DateTime)
                    return "DateTime";
                if (value is decimal || value is int || value is double)
                    return "Number";
                if (value is bool)
                    return "Boolean";
                if (value is Guid || value is List<Guid> || value is List<Guid?> || value is List<int> || value is List<int?>)
                    return "Entity";
                if (value is int[] || value is int?[])
                    throw new Exception("Trying to provide an attribute with a value which is an int-array. This is not allowed - ask the iJungleboy.");
                return "String";
            }
        }

        #endregion



        public static Dictionary<string, IAttribute> Copy(this IDictionary<string, IAttribute> attributes) 
            => attributes.ToDictionary(x => x.Key, x => x.Value.Copy());

        #region Helper to add a value with languages to an existing list of Attributes
        /// <summary>
        /// Add a value to the attribute specified. To do so, set the name, type and string of the value, as 
        /// well as some language properties.
        /// </summary>
        public static IValue AddValue(this Dictionary<string, IAttribute> target, string attributeName,
            object value, string valueType, string language = null, bool languageReadOnly = false,
            bool resolveHyperlink = false)
        {
            // pre-convert links if necessary...
            if (resolveHyperlink && valueType == AttributeTypeEnum.Hyperlink.ToString())
            {
                var valueConverter = Factory.Resolve<IEavValueConverter>();
                value = valueConverter.Convert(ConversionScenario.ConvertFriendlyToData, valueType, (string)value);
            }

            // sometimes language is passed in as an empty string - this would have side effects, so it must be neutralized
            if (string.IsNullOrWhiteSpace(language)) language = null;

            var valueWithLanguages = Value.Build(valueType, value, language == null
                ? null : new List<ILanguage> { new Dimension { Key = language, ReadOnly = languageReadOnly } });


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
    }
}
