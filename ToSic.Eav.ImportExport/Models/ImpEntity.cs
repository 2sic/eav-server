using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Implementations.ValueConverter;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.ImportExport.Models
{
    public class ImpEntity: Entity
    {
        public bool ForceNoBranch { get; set; }

        public ImpEntity(string contentTypeName): base(0, contentTypeName, null)
        {
            ForceNoBranch = false;
        }

        /// <summary>
        /// Create a new Entity. Used to create InMemory Entities that are not persisted to the EAV SqlStore.
        /// </summary>
        public ImpEntity(Guid entityGuid, string contentTypeName, IDictionary<string, object> values) : base(0, contentTypeName, values)
        {
            EntityGuid = entityGuid;
        }


        #region helpers for processing


        /// <summary>
        /// Get the value of an attribute in the language specified.
        /// </summary>
        public IValue ImpGetValueItemOfLanguage(string key, string language)
        {
            var values = Attributes
                .Where(item => item.Key == key)
                .Select(item => item.Value)
                .FirstOrDefault(); // impEntity.ValueValues(valueName);
            return values?.Values.FirstOrDefault(value => value.Languages.Any(dimension => dimension.Key == language));
        }



        /// <summary>
        /// Add a value to the attribute specified. To do so, set the name, type and string of the value, as 
        /// well as some language properties.
        /// </summary>
        public static IValue AppendAttributeValue(Dictionary<string, IAttribute> target, string attributeName,
            object value, string valueType, string language = null, bool languageReadOnly = false,
            bool resolveHyperlink = false)
        {
            // pre-convert links if necessary...
            if (resolveHyperlink && valueType == AttributeTypeEnum.Hyperlink.ToString())
            {
                var valueConverter = Factory.Resolve<IEavValueConverter>();
                value = valueConverter.Convert(ConversionScenario.ConvertFriendlyToData, valueType, (string)value);
            }

            var valueModel = Value.Build(valueType, value, language == null 
                ? null : new List<ILanguage> {new Dimension {  Key = language, ReadOnly = languageReadOnly}});

            // todo: merge the next two lines to use Value.Build instead...
           //var valueModel = ImpBuildTypedImpValueWithoutDimensions(value, valueType/*, resolveHyperlink*/);

           // if (language != null)
           //     valueModel.Languages.Add(new Dimension { Key = language, ReadOnly = languageReadOnly });//.AddLanguageReference(language, languageReadOnly);
            // end todo

            // add or replace to the collection
            var attrExists = target.Where(item => item.Key == attributeName).Select(item => item.Value).FirstOrDefault();
            if (attrExists == null)
            {
                var newAttr = AttributeBase.CreateTypedAttribute(attributeName, valueType);
                newAttr.Values.Add(valueModel);
                target.Add(attributeName, newAttr);
            }
            else
                target[attributeName].Values.Add(valueModel);

            return valueModel;
        }

        //private static string ImpTryToResolveLink(string valueString, AttributeTypeEnum valueType)
        //{
        //    var valueConverter = Factory.Resolve<IEavValueConverter>();
        //    return valueConverter.Convert(ConversionScenario.ConvertFriendlyToData, valueType.ToString(), valueString);
        //}

        //private static AttributeTypeEnum ImpFindTypeOnEnumOrUndefined(string attributeType)
        //{
        //    var type = AttributeTypeEnum.Undefined;
        //    if (attributeType != null && Enum.IsDefined(typeof(AttributeTypeEnum), attributeType))
        //        type = (AttributeTypeEnum)Enum.Parse(typeof(AttributeTypeEnum), attributeType);
        //    return type;
        //}


        //public IValue ImpPrepareTypedValue(string value, string attributeType, List<ILanguage> dimensions)
        //{
        //    var valueModel = ImpBuildTypedImpValueWithoutDimensions(value, attributeType);
        //    foreach (var dimension in dimensions)
        //    {
        //        valueModel.Languages.Add(dimension);
        //    }
        //    //valueModel.Languages = dimensions;
        //    return valueModel;
        //}

        //private static IValue ImpBuildTypedImpValueWithoutDimensions(string value, string valueType/*, bool resolveHyperlink = false*/)
        //{
        //    //ImpEntity parentEntity = this;
        //    IValue typedModel;

        //    var type = ImpFindTypeOnEnumOrUndefined(valueType);

        //    // special case hyperlink & resolve - preconvert if possible
        //    //if (resolveHyperlink && type == AttributeTypeEnum.Hyperlink)
        //    //{
        //    //    var valueConverter = Factory.Resolve<IEavValueConverter>();
        //    //    value = valueConverter.Convert(ConversionScenario.ConvertFriendlyToData, type.ToString(),
        //    //            value);
        //    //}

        //    switch (type)
        //    {
        //        case AttributeTypeEnum.String:  
        //        case AttributeTypeEnum.Hyperlink:
        //        case AttributeTypeEnum.Undefined:
        //        case AttributeTypeEnum.Empty:
        //        case AttributeTypeEnum.Custom: // ready in Build (...)
        //            typedModel = new Value<string>(value);
        //            break;
        //        case AttributeTypeEnum.Number: // ready in Build(...)
        //            decimal typedDecimal;
        //            var isDecimal = decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out typedDecimal);
        //            decimal? typedDecimalNullable = null;
        //            if (isDecimal)
        //                typedDecimalNullable = typedDecimal;
        //            typedModel = new Value<decimal?>(typedDecimalNullable);
        //            break;
        //        case AttributeTypeEnum.Entity: // ready in Build(...)
        //            var entityGuids = !IsNullOrEmpty(value)
        //                ? value.Split(',').Select(v =>
        //                {
        //                    if (v == Constants.EmptyRelationship) // this is the case when an export contains a list with nulls
        //                        return new Guid?();
        //                    var guid = Guid.Parse(v);
        //                    return guid == Guid.Empty ? new Guid?() : guid;
        //                }).ToList()
        //                : new List<Guid?>(0);
        //            typedModel = new Value<List<Guid?>>(entityGuids);
        //            break;
        //        case AttributeTypeEnum.DateTime: // ready in Build(...)
        //            DateTime typedDateTime;
        //            typedModel = new Value<DateTime?>(DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out typedDateTime)
        //                        ? typedDateTime
        //                        : new DateTime?());
        //            break;
        //        case AttributeTypeEnum.Boolean: // ready in Build(...)
        //            bool typedBoolean;
        //            typedModel = new Value<bool?>(bool.TryParse(value, out typedBoolean) ? typedBoolean : new bool?());
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException(type.ToString(), value, "Unexpected type argument found in import XML.");
        //    }
        //    return typedModel;
        //}

        //public static string ImpConvertValueObjectToString(object value)
        //{
        //    // 2017-06-06 2rm seems to do the same as ToSic.Eav.HelpersToRefactor.SerializeValue
        //    if (value == null) return null;
        //    if (value is string) return value as string;
        //    //if (!(value is IEnumerable)) return value.ToString();
        //    if (!(value is IEnumerable)) return HelpersToRefactor.SerializeValue(value);

        //    // handle relationship references - a list of GUIDs or nulls - should be csv in the end
        //    var enumerable = value as IEnumerable;

        //    var valueString = "";
        //    foreach (var item in enumerable)
        //    {
        //        // should actually be something like
        //        // (item as Newtonsoft.Json.Linq.JValue).Type == Newtonsoft.Json.Linq.JTokenType.Null
        //        // but I don't want these are libraries  in this project / temp-refactoring
        //        valueString += (item.ToString() == "" ? Constants.EmptyRelationship : item) + ",";
        //    }
        //    return valueString.Trim(','); // rmv trailing/leading commas
        //}

        #endregion
    }
}