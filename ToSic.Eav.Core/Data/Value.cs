using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a Value
    /// </summary>
    public class Value
    {
        public IList<ILanguage> Languages { get; private set; }

        /// <summary>
        /// Creates a Typed Value Model
        /// </summary>
        public static IValue Build(string attributeType, object value, List<ILanguage> languages, IDeferredEntitiesList fullEntityListForLookup = null)
        {
            if (languages == null) languages = new List<ILanguage>();
            Value typedModel;
            var stringValue = value as string;
            try
            {
                var type = (AttributeTypeEnum)Enum.Parse(typeof(AttributeTypeEnum), attributeType);
                switch (type)
                {
                    case AttributeTypeEnum.Boolean: 
                        // old, simpler: typedModel = new Value<bool?>(string.IsNullOrEmpty(stringValue) ? (bool?)null : bool.Parse(stringValue));
                        bool typedBoolean;
                        typedModel = new Value<bool?>(value as bool? ?? (bool.TryParse(stringValue, out typedBoolean) 
                            ? typedBoolean 
                            : new bool?()));
                        break;
                    case AttributeTypeEnum.DateTime: 
                        // old, simpler: typedModel = new Value<DateTime?>(string.IsNullOrEmpty(stringValue) ? (DateTime?)null : DateTime.Parse(stringValue));
                        DateTime typedDateTime;
                        typedModel = new Value<DateTime?>(value as DateTime? ?? (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture,
                                                     DateTimeStyles.None, out typedDateTime)
                                                     ? typedDateTime
                                                     : new DateTime?()));
                        break;
                    case AttributeTypeEnum.Number:
                        // old, simpler: typedModel = new Value<decimal?>(string.IsNullOrEmpty(stringValue) ? (decimal?)null : decimal.Parse(stringValue, CultureInfo.InvariantCulture));
                        decimal? typedDecimalNullable = value as decimal?;
                        if (typedDecimalNullable == null)
                        {
                            decimal typedDecimal;
                            var isDecimal = decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture,
                                out typedDecimal);
                            if (isDecimal)
                                typedDecimalNullable = typedDecimal;
                        }
                        typedModel = new Value<decimal?>(typedDecimalNullable);

                        break;
                    case AttributeTypeEnum.Entity:
                        // older, simpler
                        //var entityIds = value as IEnumerable<int?>;
                        //typedModel = new Value<EntityRelationship>(new EntityRelationship(fullEntityListForLookup, entityIds));
                        var entityIds = value as IEnumerable<int?>;
                        if (entityIds != null)
                            typedModel = new Value<EntityRelationship>(new EntityRelationship(fullEntityListForLookup, entityIds));
                        else
                        {
                            var entityIdEnum = value as IEnumerable; // note: strings are also enum!
                            if (value is string && !String.IsNullOrEmpty(stringValue))
                                entityIdEnum = stringValue.Split(',').ToList();
                            // this is the case when we get a CSV-string with GUIDs
                            var entityGuids = entityIdEnum != null //!String.IsNullOrEmpty(stringValue)
                                ? /*stringValue.Split(',')*/entityIdEnum.Cast<object>().Select(x =>
                                {
                                    var v = x.ToString().Trim();
                                    // this is the case when an export contains a list with nulls as a special code
                                    if (v == Constants.EmptyRelationship)
                                        return new Guid?();
                                    var guid = Guid.Parse(v);
                                    return guid == Guid.Empty ? new Guid?() : guid;
                                }).ToList()
                                : new List<Guid?>(0);
                            typedModel = new Value<List<Guid?>>(entityGuids);
                        }
                        break;
                    // ReSharper disable RedundantCaseLabel
                    case AttributeTypeEnum.String:  // most common case
                    case AttributeTypeEnum.Empty:   // empty - should actually not contain anything!
                    case AttributeTypeEnum.Custom:  // custom value, currently just parsed as string for manual processing as needed
                    case AttributeTypeEnum.Hyperlink:// special case, handled as string
                    case AttributeTypeEnum.Undefined:// backup case, where it's not known...
                    // ReSharper restore RedundantCaseLabel
                    default:
                        typedModel = new Value<string>(stringValue);
                        break;
                }
            }
            catch
            {
                return new Value<string>(stringValue);
            }

            typedModel.Languages = languages;

            return (IValue)typedModel;
        }


        internal static readonly Value<EntityRelationship> NullRelationship = new Value<EntityRelationship>(new EntityRelationship(null))
        {
            Languages = new List<ILanguage>()
        };

    }
}