using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ToSic.Eav.Data.Builder
{
    public static class ValueBuilder
    {
        /// <summary>
        /// Creates a Typed Value Model
        /// </summary>
        public static IValue Build(string attributeType, object value, List<ILanguage> languages,
            IEntitiesSource fullEntityListForLookup = null)
            => Build((ValueTypes)Enum.Parse(typeof(ValueTypes), attributeType), value, languages, fullEntityListForLookup);


        /// <summary>
        /// Creates a Typed Value Model
        /// </summary>
        /// <returns>
        /// An IValue, which is actually an IValue<string>, IValue<decimal>, IValue<IEnumerable<IEntity>> etc.
        /// </returns>
        public static IValue Build(ValueTypes type, object value, List<ILanguage> languages, IEntitiesSource fullEntityListForLookup = null)
        {
            if (languages == null) languages = new List<ILanguage>();
            IValue typedModel;
            var stringValue = value as string;
            try
            {
                switch (type)
                {
                    case ValueTypes.Boolean:
                        typedModel = new Value<bool?>(value as bool? ?? (bool.TryParse(stringValue, out var typedBoolean)
                            ? typedBoolean
                            : new bool?()));
                        break;
                    case ValueTypes.DateTime:
                        typedModel = new Value<DateTime?>(value as DateTime? ?? (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture,
                                                     DateTimeStyles.None, out var typedDateTime)
                                                     ? typedDateTime
                                                     : new DateTime?()));
                        break;

                    case ValueTypes.Number:
                        decimal? newDec = null;
                        if(value != null && !(value is string s && string.IsNullOrEmpty(s)))
                        {
                            // only try converting if it's not an empty string
                            try
                            {
                                newDec = System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                            }
                            catch { /* ignored */ }
                        }

                        typedModel = new Value<decimal?>(newDec);
                        break;

                    case ValueTypes.Entity:
                        IEnumerable<IEntity> rel;
                        var entityIds = value as IEnumerable<int?> ?? (value as IEnumerable<int>)
                                        ?.Select(x => (int?) x).ToList();
                        if (entityIds != null)
                            rel = new LazyEntities(fullEntityListForLookup, entityIds.ToList());
                        else if (value is IEnumerable<IEntity> relList)
                            //var lazy = (LazyEntities) relList;
                            //rel = lazy.Guids != null
                            //    ? new LazyEntities(fullEntityListForLookup, lazy.Guids)
                            //    : new LazyEntities(fullEntityListForLookup, lazy.EntityIds);
                            rel = new LazyEntities(fullEntityListForLookup, ((LazyEntities)relList).Identifiers);
                        else if (value is List<Guid?> guids)
                            rel = new LazyEntities(fullEntityListForLookup, guids);
                        else
                            rel = new LazyEntities(fullEntityListForLookup, GuidCsvToList(value)); 
                        typedModel = new Value<IEnumerable<IEntity>>(rel);
                        break;
                    // ReSharper disable RedundantCaseLabel
                    case ValueTypes.String:  // most common case
                    case ValueTypes.Empty:   // empty - should actually not contain anything!
                    case ValueTypes.Custom:  // custom value, currently just parsed as string for manual processing as needed
                    case ValueTypes.Hyperlink:// special case, handled as string
                    case ValueTypes.Undefined:// backup case, where it's not known...
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



        /// <summary>
        /// Generate a new empty relationship. This is important, because it's used often to create empty relationships...
        /// ...and then it must be a new object every time, 
        /// because the object could be changed at runtime, and if it were shared, then it would be changed in many places
        /// </summary>
        internal static Value<IEnumerable<IEntity>> NullRelationship 
            => new Value<IEnumerable<IEntity>>(new LazyEntities(null, identifiers: null))
        {
            Languages = new List<ILanguage>()
        };
    }
}
