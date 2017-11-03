﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ToSic.Eav.Enums;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data.Builder
{
    public static class ValueBuilder
    {
        /// <summary>
        /// Creates a Typed Value Model
        /// </summary>
        public static IValue Build(string attributeType, object value, List<ILanguage> languages,
            IDeferredEntitiesList fullEntityListForLookup = null)
            => Build((AttributeTypeEnum)Enum.Parse(typeof(AttributeTypeEnum), attributeType), value, languages, fullEntityListForLookup);


        /// <summary>
        /// Creates a Typed Value Model
        /// </summary>
        public static IValue Build(AttributeTypeEnum type, object value, List<ILanguage> languages, IDeferredEntitiesList fullEntityListForLookup = null)
        {
            if (languages == null) languages = new List<ILanguage>();
            Value typedModel;
            var stringValue = value as string;
            try
            {
                switch (type)
                {
                    case AttributeTypeEnum.Boolean:
                        typedModel = new Value<bool?>(value as bool? ?? (Boolean.TryParse(stringValue, out var typedBoolean)
                            ? typedBoolean
                            : new bool?()));
                        break;
                    case AttributeTypeEnum.DateTime:
                        typedModel = new Value<DateTime?>(value as DateTime? ?? (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture,
                                                     DateTimeStyles.None, out var typedDateTime)
                                                     ? typedDateTime
                                                     : new DateTime?()));
                        break;

                    case AttributeTypeEnum.Number:
                        decimal? newDec = null;
                        if (!(value is string && String.IsNullOrEmpty(value as string))) // only try converting if it's not an empty string
                        {
                            try
                            {
                                newDec = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                // ignored
                            }

                            // if it's still blank, try another method
                            // note: 2dm added this 2017-08-29 after a few bugs, but it may not be necessary any more, as the bug was something else
                            if (newDec == null)
                                try
                                {
                                    if (Decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal typedDecimal))
                                        newDec = typedDecimal;
                                }
                                catch
                                {
                                    // ignored
                                }
                        }
                        typedModel = new Value<decimal?>(newDec);
                        break;

                    case AttributeTypeEnum.Entity:
                        var entityIds = value as IEnumerable<int?> ?? (value as IEnumerable<int>)?.Select(x => (int?)x).ToList();
                        EntityRelationship rel;
                        if (entityIds != null)
                            rel = new EntityRelationship(fullEntityListForLookup, entityIds.ToList());
                        else if (value is EntityRelationship)
                            rel = ((EntityRelationship)value).Guids != null
                                ? new EntityRelationship(fullEntityListForLookup, ((EntityRelationship)value).Guids)
                                : new EntityRelationship(fullEntityListForLookup, ((EntityRelationship)value).EntityIds);
                        else if (value is List<Guid?>)
                            rel = new EntityRelationship(fullEntityListForLookup, (List<Guid?>)value);
                        else
                            rel = new EntityRelationship(fullEntityListForLookup, GuidCsvToList(value)); // new Value<List<Guid?>>(entityGuids);
                        typedModel = new Value<EntityRelationship>(rel);
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
                typedModel = new Value<string>(stringValue);
            }

            typedModel.Languages = languages;

            return (IValue)typedModel;
        }


        internal static List<Guid?> GuidCsvToList(object value)
        {
            var stringValue = value as string;
            var entityIdEnum = value as IEnumerable; // note: strings are also enum!
            if (value is string && !String.IsNullOrEmpty(stringValue))
                entityIdEnum = stringValue.Split(',').ToList();
            // this is the case when we get a CSV-string with GUIDs
            var entityGuids = entityIdEnum?.Cast<object>().Select(x =>
            {
                var v = x?.ToString().Trim();
                // this is the case when an export contains a list with nulls as a special code
                if (v == null || v == Constants.EmptyRelationship)
                    return new Guid?();
                var guid = Guid.Parse(v);
                return guid == Guid.Empty ? new Guid?() : guid;
            }).ToList() ?? new List<Guid?>(0);
            return entityGuids;
        }



        internal static readonly Value<EntityRelationship> NullRelationship = new Value<EntityRelationship>(new EntityRelationship(null, identifiers: null))
        {
            Languages = new List<ILanguage>()
        };
    }
}
