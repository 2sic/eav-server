using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ToSic.Eav.ImportExport.Interfaces;
using ToSic.Eav.ImportExport.Models;
using static System.Decimal;

namespace ToSic.Eav.Import
{
    public class ValueImportModel<T> : IValueImportModel
    {
        public T Value { get; set; }
        public IEnumerable<ImpDims> ValueDimensions { get; set; }
        public ImpEntity ParentEntity { get; }

        public ValueImportModel(ImpEntity parentEntity)
        {
            ParentEntity = parentEntity;
        }

        public string StringValueForTesting => Value.ToString();

        #region previously external stuff
        public List<IValueImportModel> ToList()
        {
            var list = new List<IValueImportModel>();
            list.Add(this);
            return list;
        }

        /// <summary>
        /// Append a language reference (ValueDimension) to this value (ValueImportModel).
        /// </summary>
        public void AppendLanguageReference(string language, bool readOnly)
        {
            var valueDimesnions = ValueDimensions as List<ImpDims>;
            if (valueDimesnions == null)
            {
                valueDimesnions = new List<ImpDims>();
                ValueDimensions = valueDimesnions;
            }

            if (!string.IsNullOrEmpty(language))
            {
                valueDimesnions.Add
                (
                    new ImpDims { DimensionExternalKey = language, ReadOnly = readOnly }
                );
            }
        }
        #endregion
    }

    public static class ValueImportModel
    {   
        public static IValueImportModel GetModel(string value, string attributeType, IEnumerable<ImpDims> dimensions, ImpEntity impEntity)
        {
            var valueModel = GenerateLightValueImportModel(value, attributeType, impEntity);

            valueModel.ValueDimensions = dimensions;

            return valueModel;
        }

        public static IValueImportModel GenerateLightValueImportModel(string value, string attributeType,
            ImpEntity impEntity)
        {
            IValueImportModel valueModel;
            var type = AttributeTypeEnum.Undefined;
            if (attributeType != null && Enum.IsDefined(typeof (AttributeTypeEnum), attributeType))
                type = (AttributeTypeEnum) Enum.Parse(typeof (AttributeTypeEnum), attributeType);

            switch (type)
            {
                case AttributeTypeEnum.String:
                case AttributeTypeEnum.Hyperlink:
                case AttributeTypeEnum.Custom:
                    valueModel = new ValueImportModel<string>(impEntity) {Value = value};
                    break;
                case AttributeTypeEnum.Number:
                    decimal typedDecimal;
                    var isDecimal = TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out typedDecimal);
                    decimal? typedDecimalNullable = null;
                    if (isDecimal)
                        typedDecimalNullable = typedDecimal;
                    valueModel = new ValueImportModel<decimal?>(impEntity)
                    {
                        Value = typedDecimalNullable
                    };
                    break;
                case AttributeTypeEnum.Entity:
                    var entityGuids = !string.IsNullOrEmpty(value)
                        ? value.Split(',').Select(v =>
                        {
                            if (v == "null") // this is the case when an export contains a list with nulls
                                return new Guid?();
                            var guid = Guid.Parse(v);
                            return guid == Guid.Empty ? new Guid?() : guid;
                        }).ToList()
                        : new List<Guid?>(0);
                    valueModel = new ValueImportModel<List<Guid?>>(impEntity) {Value = entityGuids};
                    break;
                case AttributeTypeEnum.DateTime:
                    DateTime typedDateTime;
                    valueModel = new ValueImportModel<DateTime?>(impEntity)
                    {
                        Value =
                            DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out typedDateTime)
                                ? typedDateTime
                                : new DateTime?()
                    };
                    break;
                case AttributeTypeEnum.Boolean:
                    bool typedBoolean;
                    valueModel = new ValueImportModel<bool?>(impEntity)
                    {
                        Value = bool.TryParse(value, out typedBoolean) ? typedBoolean : new bool?()
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(attributeType, value, "Unknown type argument found in import XML.");
            }
            return valueModel;
        }
    }
}