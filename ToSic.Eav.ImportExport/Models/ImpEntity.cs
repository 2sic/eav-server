using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ToSic.Eav.Implementations.ValueConverter;
using ToSic.Eav.ImportExport.Interfaces;
using Microsoft.Practices.Unity;
using static System.String;

namespace ToSic.Eav.ImportExport.Models
{
    public class ImpEntity
    {
        // Type and IDs
        public string AttributeSetStaticName { get; set; }
        public Guid? EntityGuid { get; set; }

        // Keys
        public int? KeyNumber { get; set; }
        public Guid? KeyGuid { get; set; }
        public string KeyString { get; set; }
        public int KeyTypeId { get; set; }

        // Draft / Publish / Don't allow Draft
        public bool IsPublished { get; set; }
        public bool ForceNoBranch { get; set; }

        public Dictionary<string, List<IImpValue>> Values { get; set; }

        public ImpEntity()
        {
            IsPublished = true;
            ForceNoBranch = false;
        }


        #region helpers for processing


        /// <summary>
        /// Get the value of an attribute in the language specified.
        /// </summary>
        public IImpValue GetValueItemOfLanguage(string key, string language)
        {
            var values = Values
                .Where(item => item.Key == key)
                .Select(item => item.Value)
                .FirstOrDefault(); // impEntity.ValueValues(valueName);
            return values?.FirstOrDefault(value => value.ValueDimensions.Any(dimension => dimension.DimensionExternalKey == language));
        }



        /// <summary>
        /// Add a value to the attribute specified. To do so, set the name, type and string of the value, as 
        /// well as some language properties.
        /// </summary>
        public IImpValue AppendAttributeValue(string attributeName, string value, string valueType, string language = null, bool languageReadOnly = false, bool resolveHyperlink = false)
        {
            var valueModel = BuildTypedImpValueWithoutDimensions(value, valueType, resolveHyperlink);

            if (language != null)
                valueModel.AppendLanguageReference(language, languageReadOnly);

            // add or replace...
            var attrExists = Values.Where(item => item.Key == attributeName).Select(item => item.Value).FirstOrDefault();
            if (attrExists == null)
                Values.Add(attributeName, new List<IImpValue> { valueModel });
            else
                Values[attributeName].Add(valueModel);

            return valueModel;
        }

        private static string TryToResolveLink(string valueString, AttributeTypeEnum valueType)
        {
            var valueConverter = Factory.Container.Resolve<IEavValueConverter>();
            return valueConverter.Convert(ConversionScenario.ConvertFriendlyToData, valueType.ToString(), valueString);
        }

        private static AttributeTypeEnum FindTypeOnEnumOrUndefined(string attributeType)
        {
            var type = AttributeTypeEnum.Undefined;
            if (attributeType != null && Enum.IsDefined(typeof(AttributeTypeEnum), attributeType))
                type = (AttributeTypeEnum)Enum.Parse(typeof(AttributeTypeEnum), attributeType);
            return type;
        }


        public IImpValue PrepareTypedValue(string value, string attributeType, List<ImpDims> dimensions)
        {
            var valueModel = BuildTypedImpValueWithoutDimensions(value, attributeType);
            valueModel.ValueDimensions = dimensions;
            return valueModel;
        }

        private IImpValue BuildTypedImpValueWithoutDimensions(string value, string valueType, bool resolveHyperlink = false)
        {
            ImpEntity parentEntity = this;
            IImpValue impValueModel;

            var type = FindTypeOnEnumOrUndefined(valueType);

            // special case hyperlink & resolve - preconvert if possible
            if (resolveHyperlink && type == AttributeTypeEnum.Hyperlink)
                value = TryToResolveLink(value, type);

            switch (type)
            {
                case AttributeTypeEnum.String:
                case AttributeTypeEnum.Hyperlink:
                case AttributeTypeEnum.Undefined:   // note: added 2017-04-05 by 2dm, should be correct, but may not throw important errors now
                case AttributeTypeEnum.Empty:       // note: added 2017-04-05 by 2dm, should be correct, but may not throw important errors now
                case AttributeTypeEnum.Custom:
                    impValueModel = new ImpValue<string>(parentEntity, value);
                    break;
                case AttributeTypeEnum.Number:
                    decimal typedDecimal;
                    var isDecimal = decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out typedDecimal);
                    decimal? typedDecimalNullable = null;
                    if (isDecimal)
                        typedDecimalNullable = typedDecimal;
                    impValueModel = new ImpValue<decimal?>(parentEntity, typedDecimalNullable);
                    break;
                case AttributeTypeEnum.Entity:
                    var entityGuids = !IsNullOrEmpty(value)
                        ? value.Split(',').Select(v =>
                        {
                            if (v == "null") // this is the case when an export contains a list with nulls
                                return new Guid?();
                            var guid = Guid.Parse(v);
                            return guid == Guid.Empty ? new Guid?() : guid;
                        }).ToList()
                        : new List<Guid?>(0);
                    impValueModel = new ImpValue<List<Guid?>>(parentEntity, entityGuids);
                    break;
                case AttributeTypeEnum.DateTime:
                    DateTime typedDateTime;
                    impValueModel = new ImpValue<DateTime?>(parentEntity, DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out typedDateTime)
                                ? typedDateTime
                                : new DateTime?());
                    break;
                case AttributeTypeEnum.Boolean:
                    bool typedBoolean;
                    impValueModel = new ImpValue<bool?>(parentEntity, bool.TryParse(value, out typedBoolean) ? typedBoolean : new bool?());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(type.ToString(), value, "Unexpected type argument found in import XML.");
            }
            return impValueModel;
        }

        public static string ConvertValueObjectToString(object value)
        {
            if (value == null) return null;
            if (value is string) return value as string;
            if (!(value is IEnumerable)) return value.ToString();

            // handle relationship references - a list of GUIDs or nulls - should be csv in the end
            var enumerable = value as IEnumerable;

            var valueString = "";
            foreach (var item in enumerable)
            {
                // warning: 2dm testing 2016-01-27 
                // should actually be something like
                // (item as Newtonsoft.Json.Linq.JValue).Type == Newtonsoft.Json.Linq.JTokenType.Null
                // but these are libraries I don't want in this project / temp-refactoring
                valueString += (item.ToString() == "" ? "null" : item) + ",";
            }
            return valueString.Trim(','); // rmv trailing/leading commas
        }

        #endregion
    }
}