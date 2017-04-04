using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Implementations.ValueConverter;
using ToSic.Eav.Import;
using ToSic.Eav.ImportExport.Interfaces;
using Microsoft.Practices.Unity;

namespace ToSic.Eav.ImportExport.Models
{
    public class ImpEntity
    {
        public string AttributeSetStaticName { get; set; }
        public int? KeyNumber { get; set; }
        public Guid? KeyGuid { get; set; }
        public string KeyString { get; set; }
        public int AssignmentObjectTypeId { get; set; }
        public Guid? EntityGuid { get; set; }
        public bool IsPublished { get; set; }

        public bool ForceNoBranch { get; set; }
        public Dictionary<string, List<IValueImportModel>> Values { get; set; }

        public ImpEntity()
        {
            IsPublished = true;
            ForceNoBranch = false;
        }


        #region helpers for processing


        /// <summary>
        /// Get the value of an attribute in the language specified.
        /// </summary>
        public IValueImportModel ValueOfLanguage(string valueName, string valueLanguage)
        {
            var values = Values
                .Where(item => item.Key == valueName)
                .Select(item => item.Value)
                .FirstOrDefault(); // impEntity.ValueValues(valueName);
            return values?.FirstOrDefault(value => value.ValueDimensions.Any(dimension => dimension.DimensionExternalKey == valueLanguage));
        }



        /// <summary>
        /// Add a value to the attribute specified. To do so, set the name, type and string of the value, as 
        /// well as some language properties.
        /// </summary>
        public IValueImportModel AppendAttributeValue(string valueName, string valueString, string valueType, string valueLanguage = null, bool valueReadOnly = false, bool resolveHyperlink = false)
        {
            var valueModel = GetValueModel(this, valueString, valueType, valueLanguage, valueReadOnly, resolveHyperlink);
            var entityValue = Values.Where(item => item.Key == valueName).Select(item => item.Value).FirstOrDefault();
            if (entityValue == null)
            {
                Values.Add(valueName, valueModel.ToList());
            }
            else
            {
                Values[valueName].Add(valueModel);
            }
            return valueModel;
        }

        // Todo / warning: highly duplicate code with ValueImportModel.cs
        // but can't quickly refactor, because it's still a bit different
        private IValueImportModel GetValueModel(ImpEntity impEntity, string valueString, string valueType, string valueLanguage = null, bool valueRedOnly = false, bool resolveHyperlink = false)
        {
            IValueImportModel valueModel;
            var valueConverter = Factory.Container.Resolve<IEavValueConverter>();

            var type = AttributeTypeEnum.Undefined;
            if (valueType != null && Enum.IsDefined(typeof(AttributeTypeEnum), valueType))
                type = (AttributeTypeEnum)Enum.Parse(typeof(AttributeTypeEnum), valueType);

            switch (type)
            {
                case AttributeTypeEnum.Boolean:
                    {
                        valueModel = new ValueImportModel<bool?>(impEntity)
                        {
                            Value = string.IsNullOrEmpty(valueString) ? null : new Boolean?(Boolean.Parse(valueString))
                        };
                    }
                    break;

                case AttributeTypeEnum.Number:
                    {
                        valueModel = new ValueImportModel<decimal?>(impEntity)
                        {
                            Value = string.IsNullOrEmpty(valueString) ? null : new Decimal?(Decimal.Parse(valueString))
                        };
                    }
                    break;

                case AttributeTypeEnum.DateTime:
                    {
                        valueModel = new ValueImportModel<DateTime?>(impEntity)
                        {
                            Value = string.IsNullOrEmpty(valueString) ? null : new DateTime?(DateTime.Parse(valueString.Replace("T", " ").Replace("Z", " ")))
                        };
                    }
                    break;

                case AttributeTypeEnum.Hyperlink:
                    {
                        string valueReference;
                        if (string.IsNullOrEmpty(valueString) || !resolveHyperlink)
                            valueReference = valueString;
                        else
                        {
                            valueReference = valueConverter.Convert(ConversionScenario.ConvertFriendlyToData, valueType, valueString);
                        }
                        valueModel = new ValueImportModel<string>(impEntity) { Value = valueReference };
                    }
                    break;

                case AttributeTypeEnum.Entity:
                    {
                        valueModel = ValueImportModel.GenerateLightValueImportModel(valueString, valueType, impEntity);
                    }
                    break;

                //case AttributeTypeEnum.String:
                //case AttributeTypeEnum.Custom:
                default:
                    {   // String
                        valueModel = new ValueImportModel<string>(impEntity) { Value = valueString };
                    }
                    break;
            }
            if (valueLanguage != null)
            {
                valueModel.AppendLanguageReference(valueLanguage, valueRedOnly);
            }
            return valueModel;
        }




        public IValueImportModel AppendAttributeValue(string valueName, object value, string valueType)//, string valueLanguage = null, bool valueReadOnly = false, bool resolveHyperlink = false)
        {
            string valueString;
            if (value == null)
                valueString = null;
            else if (value is string)
                valueString = value as string;
            else if (value is IEnumerable)
            {
                var enumerable = value as IEnumerable;

                valueString = "";
                foreach (var item in enumerable)
                {
                    // warning: 2dm testing 2016-01-27 
                    // should actually be something like
                    // (item as Newtonsoft.Json.Linq.JValue).Type == Newtonsoft.Json.Linq.JTokenType.Null
                    // but these are libraries I don't want in this project / temp-refactoring
                    valueString += (item.ToString() == "" ? "null" : item) + ",";
                }
                valueString = valueString.Trim(','); // rmv trailing/leading commas
            }
            else
                valueString = value.ToString();

            return AppendAttributeValue(valueName, valueString, valueType, null, false, false);

        }

        #endregion
    }
}